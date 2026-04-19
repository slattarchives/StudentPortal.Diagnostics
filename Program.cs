using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentPortal.Diagnostics.Middleware;
using StudentPortal.Diagnostics.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStudentPortalServices();
var serviceDescriptors = builder.Services;

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Use(async (context, next) =>
{
    var counter = context.RequestServices.GetRequiredService<IRequestCounterService>();
    int reqNum = counter.IncrementAndGet();
    
    Console.WriteLine($"[#{reqNum}] [START] {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"[#{reqNum}] [END] {context.Request.Path} -> {context.Response.StatusCode}");
});

app.UseWhen(ctx => ctx.Request.Query["trace"] == "true", branch =>
{
    branch.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Debug-Trace", "enabled");
        await next();
    });
});

app.MapWhen(ctx => ctx.Request.Query["format"] == "plain", branch =>
{
    branch.Run(async context =>
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Plain text response from MapWhen");
    });
});

var tools = app.MapGroup("/tools");
tools.MapGet("/time", (IDateTimeService svc) => $"Текущее время: {svc.GetTime()}");
tools.MapGet("/date", (IDateTimeService svc) => $"Текущая дата: {svc.GetDate()}");
tools.MapGet("/info", (IAppVersionService verSvc) => Results.Text(
    $"StudentPortal.Diagnostics\nВерсия: {verSvc.GetVersion()}\n{verSvc.GetBuildInfo()}",
    "text/plain; charset=utf-8"));

app.Map("/secure", secureApp =>
{
    secureApp.UseToken("study2026");
    secureApp.Run(async context =>
    {
        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync("Секретный отчёт: доступ разрешён.");
    });
});

app.MapGet("/", () => Results.Text(
    "Добро пожаловать в StudentPortal.Diagnostics!\n" +
    "Маршруты: /tools/time, /tools/date, /tools/info, /secure/report?token=study2026, /env, /di/services",
    "text/plain; charset=utf-8"));

app.MapGet("/env", (IWebHostEnvironment env) => Results.Text(
    $"Environment: {env.EnvironmentName}\n" +
    $"Application: {env.ApplicationName}\n" +
    $"ContentRoot: {env.ContentRootPath}\n" +
    $"WebRoot: {(string.IsNullOrEmpty(env.WebRootPath) ? "Не задан" : env.WebRootPath)}",
    "text/plain; charset=utf-8"));

app.MapGet("/di/services", () =>
{
    var list = serviceDescriptors.Take(10).Select(s => $"{s.ServiceType.Name} [{s.Lifetime}] -> {s.ImplementationType?.Name ?? "Delegate"}");
    return Results.Text($"Всего сервисов: {serviceDescriptors.Count}\n\nПервые 10:\n{string.Join("\n", list)}", "text/plain; charset=utf-8");
});

app.Run();