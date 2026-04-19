using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace StudentPortal.Diagnostics.Middleware;

public class TokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _validToken;
    public TokenMiddleware(RequestDelegate next, string pattern)
    {
        _next = next;
        _validToken = pattern;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Query["token"].FirstOrDefault();
        if (string.IsNullOrEmpty(token) || token != _validToken)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
        await _next(context);
    }
}