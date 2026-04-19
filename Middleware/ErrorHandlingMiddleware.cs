using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace StudentPortal.Diagnostics.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.HasStarted) return;

        if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("[ErrorHandling] Доступ запрещён (403). Проверьте параметр token.");
        }
        else if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("[ErrorHandling] Ресурс не найден (404).");
        }
    }
}