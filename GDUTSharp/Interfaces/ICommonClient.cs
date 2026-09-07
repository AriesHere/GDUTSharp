namespace GDUTSharp.Interfaces;

public interface ICommonClient
{
    public Task<HttpResponseMessage> Send(HttpRequestMessage request, CancellationToken cancellationToken = default);

    public HttpRequestMessage CreateRequest(HttpMethod method, string url, Dictionary<string, string> content, string? referer = null);

    public void DisposeRequest(HttpRequestMessage request);
}
