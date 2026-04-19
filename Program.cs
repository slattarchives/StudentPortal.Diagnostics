using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentPortal.Diagnostics.Middleware;
using StudentPortal.Diagnostics.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрация DI (до Build)
builder.Services.AddStudentPortalServices();

// Сохраняем коллекцию, чтобы вывести её в /di/services
var serviceDescriptors = builder.Services;

var app = builder.Build();

// 2. ErrorHandlingMiddleware (ВСЕГДА первый по п.7)
app.UseMiddleware<ErrorHandlingMiddleware>();

// 3. Inline middleware (оборачивает всё последующее)
app.Use(async (context, next) =>
{
    Console.WriteLine($"[START] {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[END]   {context.Request.Path} -> {context.Response.StatusCode}");
});

// 4. UseWhen (trace=true) -> возврат в основной pipeline
app.UseWhen(ctx => ctx.Request.Query["trace"] == "true", branch =>
{
    branch.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Debug-Trace", "enabled");
        Console.WriteLine($"[TRACE] 📡 Обработка запроса с trace=true");
        await next();
    });
});

// 5. MapWhen (format=plain) -> изолированная ветка, не возвращается
app.MapWhen(ctx => ctx.Request.Query["format"] == "plain", branch =>
{
    branch.Run(async context =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("📄 Plain text response from MapWhen (pipeline прерван)");
    });
});
// 6. Ветка /tools (вложенные маршруты)
app.Map("/tools", toolsApp =>
{
    // В .NET 10 внутри ветки IApplicationBuilder маршруты добавляются через UseEndpoints
    toolsApp.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/time", (IDateTimeService svc) => $"⏰ Текущее время: {svc.GetTime()}");
        endpoints.MapGet("/date", (IDateTimeService svc) => $"📅 Текущая дата: {svc.GetDate()}");
        endpoints.MapGet("/info", () => "🛠 StudentPortal.Diagnostics v1.0 | Tools section");
    });
});

// 7. Ветка /secure с TokenMiddleware ПЕРЕД обработчиком
app.Map("/secure", secureApp =>
{
    // Подключаем ваш extension method (работает с IApplicationBuilder)
    secureApp.UseToken("study2026");
    
    secureApp.UseEndpoints(endpoints =>
    {
        endpoints.MapGet("/report", () => "🔐 Секретный отчёт: доступ разрешён.");
    });
});
// 8. Статические endpoint'ы
app.MapGet("/", () => "🏠 Добро пожаловать в StudentPortal.Diagnostics!\n" +
                      "Маршруты: /tools/time, /tools/date, /tools/info, /secure/report?token=study2026, /env, /di/services");

app.MapGet("/env", (IWebHostEnvironment env) =>
{
    var summary = $"🌍 EnvironmentName: {env.EnvironmentName}\n" +
                  $"📦 ApplicationName: {env.ApplicationName}\n" +
                  $"📁 ContentRootPath: {env.ContentRootPath}\n" +
                  $"🌐 WebRootPath: {(string.IsNullOrEmpty(env.WebRootPath) ? "Не задан" : env.WebRootPath)}\n" +
                  $"✅ IsDevelopment: {env.IsDevelopment()}\n" +
                  $"✅ IsProduction: {env.IsProduction()}";
    return summary;
});

app.MapGet("/di/services", () =>
{
    var list = serviceDescriptors.ToList();
    var output = $"📊 Всего зарегистрированных сервисов: {list.Count}\n\n🔍 Первые 10:\n";
    foreach (var desc in list.Take(10))
    {
        output += $"• {desc.ServiceType.Name} [{desc.Lifetime}] -> {desc.ImplementationType?.Name ?? "Delegate/Instance"}\n";
    }
    return output;
});

// 9. Fallback для /unknown (гарантирует 404 тело, которое ловит ErrorHandlingMiddleware)
app.Run(async context =>
{
    context.Response.StatusCode = 404;
    await context.Response.WriteAsync("Маршрут не найден.");
});

app.Run();