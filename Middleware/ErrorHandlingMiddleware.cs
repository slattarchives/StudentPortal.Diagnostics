namespace StudentPortal.Diagnostics.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Выполняем следующую ветку pipeline
        await _next(context);

        // Проверяем статус после обработки всеми последующими компонентами
        if (context.Response.StatusCode == 403)
        {
            await context.Response.WriteAsync("\n🔍 [ErrorHandling] Доступ запрещён (403). Проверьте параметр token.");
        }
        else if (context.Response.StatusCode == 404)
        {
            await context.Response.WriteAsync("\n🔍 [ErrorHandling] Ресурс не найден (404).");
        }
    }
}