using System.Security.Cryptography;

namespace FinanzasPersonales.Api.Services
{
    public class AuthCookieService
    {
        public const string AccessCookie = "access_token";
        public const string RefreshCookie = "refresh_token";
        public const string CsrfCookie = "csrf_token";

        private readonly bool _secure;
        public AuthCookieService(IConfiguration config)
        {
            _secure = !bool.TryParse(config["Auth:UseSecureCookies"], out var s) || s; // true por defecto
        }

        private CookieOptions Base(DateTime expires, bool httpOnly, string path = "/") => new()
        {
            HttpOnly = httpOnly,
            Secure = _secure,
            SameSite = SameSiteMode.None,
            Expires = expires,
            Path = path
        };

        public void SetAuthCookies(HttpResponse res, string accessJwt, string refreshRaw, int accessMinutes, int refreshDays)
        {
            res.Cookies.Append(AccessCookie, accessJwt,
                Base(DateTime.UtcNow.AddMinutes(accessMinutes), httpOnly: true));
            res.Cookies.Append(RefreshCookie, refreshRaw,
                Base(DateTime.UtcNow.AddDays(refreshDays), httpOnly: true, path: "/api/Auth"));

            var csrf = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
            res.Cookies.Append(CsrfCookie, csrf,
                Base(DateTime.UtcNow.AddDays(refreshDays), httpOnly: false));
        }

        public void ClearAuthCookies(HttpResponse res)
        {
            foreach (var (name, path) in new[] { (AccessCookie, "/"), (RefreshCookie, "/api/Auth"), (CsrfCookie, "/") })
            {
                res.Cookies.Append(name, "", new CookieOptions
                {
                    HttpOnly = name != CsrfCookie,
                    Secure = _secure,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = path
                });
            }
        }
    }
}
