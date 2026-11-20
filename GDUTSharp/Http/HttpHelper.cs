using System.Net;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Http;

public static class HttpHelper
{
    public static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently      // 301
            or HttpStatusCode.Redirect                     // 302
            or HttpStatusCode.SeeOther                     // 303
            or HttpStatusCode.TemporaryRedirect            // 307
            or HttpStatusCode.PermanentRedirect;           // 308

    public static void FillCookies(this CookieContainer cookies, HttpRequestMessage request)
    {
        var cookieHeader = cookies!.GetCookieHeader(request.RequestUri!);
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }
    }

    public static void AddCookies(this CookieContainer cookies, ILogger? logger, HttpResponseMessage response)
    {
        bool isLoggerNotNull = logger is not null;
        if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
        {
            foreach (var cookieHeader in cookieHeaders)
            {
                try
                {
                    cookies.SetCookies(response.RequestMessage!.RequestUri!, cookieHeader);
                }
                catch (Exception e) when (isLoggerNotNull && e is CookieException or ArgumentException)
                {
                    Log.IgnoreInvalidSetCookie(logger!, cookieHeader, e);
                }
            }
        }
    }

    /// <remarks>
    /// <b>警告：</b>目前的判断逻辑仍有问题，详见单元测试，请谨慎使用。
    /// </remarks>
    public static bool IsCrossSite(Uri from, Uri to)
    {
        return Uri.Compare(from, to, UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) != 0;
    }
}
