using System.Net;

namespace GDUTSharp.Interfaces;

public interface ICommonClient
{
    public Task<HttpResponseMessage> SendAsync(CookieContainer cookies, HttpRequestMessage request, CancellationToken cancellationToken = default);

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
