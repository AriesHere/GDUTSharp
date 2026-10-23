using System.Net;

namespace GDUTSharp.Interfaces;

public interface ICommonClient
{
    /// <remarks>
    /// 如果 <paramref name="allowCrossDomainRedirect"/> 为 <see cref="true"/>，跨域重定向时仅会通过日志
    /// 发出警告，如果为 <see cref="false"/>，跨域重定向时会抛出 <see cref="InvalidOperationException"/>。
    /// 除非有必要，否则不应该将其设置为 <see cref="true"/>
    /// </remarks>
    public Task<HttpResponseMessage> SendAsync(
        CookieContainer cookies,
        HttpRequestMessage request,
        bool allowCrossDomainRedirect = false,
        CancellationToken cancellationToken = default);

    public static HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string>? content = null, string? referer = null)
    {
        var c = content != null ? new FormUrlEncodedContent(content) : null;
        var request = new HttpRequestMessage(method, url)
        {
            Content = c
        };
        if (referer != null) request.Headers.Referrer = new(referer);
        return request;
    }
}
