using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Middleware
{
    public class CsrfValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS", "TRACE" };

        public CsrfValidationMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext ctx)
        {
            var path = ctx.Request.Path;
            var method = ctx.Request.Method;

            var needsCheck = !SafeMethods.Contains(method)
                && path.StartsWithSegments("/api")
                && !path.StartsWithSegments("/api/Auth/google");

            if (needsCheck)
            {
                var cookie = ctx.Request.Cookies[AuthCookieService.CsrfCookie];
                var header = ctx.Request.Headers["X-CSRF-Token"].ToString();
                if (string.IsNullOrEmpty(cookie) || cookie != header)
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await ctx.Response.WriteAsync("CSRF token invalido.");
                    return;
                }
            }
            await _next(ctx);
        }
    }
}
