using System.Net;
using GDUTSharp.Http;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Services;

public class CommonClient(
    ILogger<CommonClient> logger,
    IOptions<HttpOptions> options,
    IHttpClientFactory factory,
    HttpConcurrencyLimiter concurrencyLimiter
    ) : ICommonClient
{
    protected readonly ILogger<CommonClient> _logger = logger;
    protected readonly IHttpClientFactory _factory = factory;
    protected readonly HttpConcurrencyLimiter _concurrencyLimiter = concurrencyLimiter;
    protected readonly int _maxRedirect = options.Value.MaxRedirect;

    public virtual async Task<HttpResponseMessage> SendAsync(
        CookieContainer cookies,
        HttpRequestMessage request,
        bool allowCrossDomainRedirect = true,
        CancellationToken token = default)
    {
        await _concurrencyLimiter.WaitAsync(token);
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(request.RequestUri?.ToString(), nameof(request.RequestUri));
            using var client = _factory.CreateClient(CommonClientExtensions.ClientName);
            var currentRequest = request;   // 用于在后文决定所有权以及是否需要 Dispose
            HttpResponseMessage response;
            var redirectCount = 0;
            while (true)
            {
                var cookieHeader = cookies.GetCookieHeader(currentRequest.RequestUri!);
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    currentRequest.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                }
                response = await client.SendAsync(currentRequest, token);
                if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                {
                    foreach (var cookieValue in setCookieValues)
                    {
                        try
                        {
                            cookies.SetCookies(currentRequest.RequestUri!, cookieValue);
                        }
                        catch (Exception e) when (e is CookieException or ArgumentException)
                        {
                            Log.IgnoreInvalidSetCookie(_logger, cookieValue, e);
                        }
                    }
                }
                if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                {
                    break;
                }
                redirectCount++;
                if (redirectCount > _maxRedirect)
                {
                    response.Dispose();
                    throw new HttpRequestException($"重定向次数超过限制 ({_maxRedirect})，目标 URI: {request.RequestUri}");
                }
                var oldUri = currentRequest.RequestUri!;
                var newUri = ResolveLocation(oldUri, response.Headers.Location);
                if (Uri.Compare(oldUri, newUri, UriComponents.Host, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (allowCrossDomainRedirect)
                    {
                        Log.CrossDomainRedirect(_logger, oldUri.AbsoluteUri, newUri.AbsoluteUri, null);
                    }
                    else
                    {
                        response.Dispose();
                        throw new InvalidOperationException($"检测到跨域重定向 {oldUri.AbsoluteUri} -> {newUri.AbsoluteUri}");
                    }
                }
                var newRequest = await CreateRedirectRequest(currentRequest, newUri, response.StatusCode, token);
                response.Dispose();
                if (!ReferenceEquals(currentRequest, request))
                {
                    currentRequest.Dispose();
                }
                currentRequest = newRequest;
            }
            return response;
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    protected static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently      // 301
            or HttpStatusCode.Redirect                     // 302
            or HttpStatusCode.SeeOther                     // 303
            or HttpStatusCode.TemporaryRedirect            // 307
            or HttpStatusCode.PermanentRedirect;           // 308

    protected static Uri ResolveLocation(Uri baseUri, Uri location) => location.IsAbsoluteUri ? location : new Uri(baseUri, location);

    protected static async Task<HttpRequestMessage> CreateRedirectRequest(HttpRequestMessage previousRequest, Uri newUri, HttpStatusCode redirectStatus, CancellationToken token)
    {
        var method = previousRequest.Method;
        if (redirectStatus == HttpStatusCode.SeeOther && method != HttpMethod.Get)
        {
            method = HttpMethod.Get;
        }
        byte[]? body = null;
        List<KeyValuePair<string, IEnumerable<string>>>? contentHeaders = null;
        if (method != HttpMethod.Get && previousRequest.Content is not null)
        {
            body = await previousRequest.Content.ReadAsByteArrayAsync(token);
            contentHeaders = [.. previousRequest.Content.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value))];
        }
        var newRequest = new HttpRequestMessage(method, newUri);
        foreach (var header in previousRequest.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        if (method != HttpMethod.Get && previousRequest.Content is not null)
        {
            newRequest.Content = previousRequest.Content;
        }
        if (body is not null)
        {
            var newContent = new ByteArrayContent(body);
            foreach (var header in contentHeaders!)
            {
                if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            newRequest.Content = newContent;
        }
        return newRequest;
    }
}

public static class CommonClientExtensions
{
    public const string ClientName = "GDUTSharp.CommonClient";

    public static IServiceCollection AddCommonClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HttpOptions>(configuration.GetSection(nameof(HttpOptions)))
            .AddSingleton<HttpConcurrencyLimiter>()
            .AddTransient<RetryHandler>()
            .AddTransient<ICommonClient, CommonClient>()
            .AddHttpClient(ClientName, (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HttpOptions>>().Value;
                client.Timeout = TimeSpan.FromMilliseconds(options.OverallTimeout);
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<HttpOptions>>().Value;
                var handler = new SocketsHttpHandler
                {
                    MaxConnectionsPerServer = options.MaxConnectionsPerServer,
                    PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(options.PooledConnectionIdleTimeout),
                    PooledConnectionLifetime = TimeSpan.FromMilliseconds(options.OverallTimeout),
                    AllowAutoRedirect = false,  // 请勿自动重定向，否则会导致重定向后的 Cookie 无法获取
                };
                return handler;
            })
            .AddHttpMessageHandler<RetryHandler>();

        return services;
    }
}

public class HttpOptions
{
    public int MaxRequests { get; set; } = 1024;

    public int MaxConnectionsPerServer { get; set; } = 128;

    /// <summary>连接池空闲超时（毫秒）</summary>
    public int PooledConnectionIdleTimeout { get; set; } = 60000;

    /// <summary>整体超时（毫秒）</summary>
    public int OverallTimeout { get; set; } = 30000;

    public int MaxRetry { get; set; } = 3;

    /// <summary>重试延迟（毫秒）</summary>
    public int RetryDelay { get; set; } = 100;

    /// <summary>最大重定向次数</summary>
    /// <remarks>不应小于 5，否则某些服务的默认实现会出问题</remarks>
    public int MaxRedirect { get; set; } = 10;
}
