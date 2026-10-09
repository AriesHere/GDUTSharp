using System.Net;
using GDUTSharp.Http;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Services;

public class CommonClient(ILogger<CommonClient> logger, IHttpClientFactory factory, HttpConcurrencyLimiter concurrencyLimiter) : ICommonClient
{
    protected readonly ILogger<CommonClient> _logger = logger;
    protected readonly IHttpClientFactory _factory = factory;
    protected readonly HttpConcurrencyLimiter _concurrencyLimiter = concurrencyLimiter;

    public virtual async Task<HttpResponseMessage> SendAsync(CookieContainer cookies, HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(request.RequestUri?.ToString(), nameof(request.RequestUri));
            var cookieHeader = cookies.GetCookieHeader(request.RequestUri);
            if (!string.IsNullOrEmpty(cookieHeader)) request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            using var client = _factory.CreateClient(CommonClientExtensions.ClientName);
            var response = await client.SendAsync(request, cancellationToken);
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieValue in setCookieValues)
                {
                    try { cookies.SetCookies(request.RequestUri, cookieValue); }
                    catch (Exception e) when (e is CookieException or ArgumentException)
                    {
                        Log.IgnoreInvalidSetCookie(_logger, cookieValue, e);
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
