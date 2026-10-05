using System.Net;
using GDUTSharp.Http;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Services;

public class CommonClient(ILogger<CommonClient> logger, HttpClient httpClient, HttpConcurrencyLimiter concurrencyLimiter) : ICommonClient
{
    protected readonly ILogger<CommonClient> _logger = logger;
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly HttpConcurrencyLimiter _concurrencyLimiter = concurrencyLimiter;

    public CookieContainer CookieContainer { get; } = new();

    public async virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            Exception.ThrowIfNull(request.RequestUri, "请求 URI 不能为 null");
            var cookieHeader = CookieContainer.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrEmpty(cookieHeader)) request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieValue in setCookieValues)
                {
                    try { CookieContainer.SetCookies(request.RequestUri!, cookieValue); }
                    catch (Exception ex) when (ex is CookieException or ArgumentException)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning)) _logger.LogWarning(ex, "忽略非法 Set-Cookie: {Cookie}", cookieValue);
                    }
                }
            }
            return response;
        }
        finally { _concurrencyLimiter.Release(); }
    }
}

public static class CommonClientExtensions
{
    public static IServiceCollection AddCommonClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HttpOptions>(configuration.GetSection(nameof(HttpOptions)))
            .AddSingleton<HttpConcurrencyLimiter>()
            .AddTransient<RetryHandler>()
            .AddHttpClient<ICommonClient, CommonClient>((sp, client) =>
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
                    AllowAutoRedirect = true,
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
}
