using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace AgentDashboard.Web.Middleware;

/// <summary>
/// Middleware to handle culture detection from query string or cookie.
/// </summary>
public class CultureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestLocalizationOptions _options;

    public CultureMiddleware(RequestDelegate next, IOptions<RequestLocalizationOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check query string first
        var cultureQuery = context.Request.Query["culture"].FirstOrDefault();
        
        if (!string.IsNullOrWhiteSpace(cultureQuery) && 
            _options.SupportedUICultures?.Any(c => c.Name == cultureQuery) == true)
        {
            var cultureInfo = new CultureInfo(cultureQuery);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            // Save to cookie for future requests
            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureInfo)),
                new CookieOptions {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });
        }
        else
        {
            // Check cookie
            var cookie = context.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                var cookieValue = CookieRequestCultureProvider.ParseCookieValue(cookie);
                if (cookieValue != null && cookieValue.Cultures != null && cookieValue.Cultures.Count > 0)
                {
                    var cultureName = cookieValue.Cultures[0].Value;
                    if (!string.IsNullOrEmpty(cultureName))
                    {
                        var cultureInfo = new CultureInfo(cultureName);
                        if (_options.SupportedUICultures?.Any(c => c.Name == cultureInfo.Name) == true)
                        {
                            CultureInfo.CurrentCulture = cultureInfo;
                            CultureInfo.CurrentUICulture = cultureInfo;
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}
