namespace StudentPortal.Diagnostics.Middleware;
public class TokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _validToken;
    
    public TokenMiddleware(RequestDelegate next, string validToken)
    {
        _next = next;
        _validToken = validToken;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Query["token"];
        if (token != _validToken)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Forbidden: Invalid token");
            return;
        }
        await _next(context);
    }
}