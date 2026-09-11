using System.Net;
using GDUTSharp.Http;
using GDUTSharp.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Services
{
    public class CommonClient(ILogger<CommonClient> logger, HttpClient httpClient, SemaphoreSlim semaphoreSlim) : ICommonClient
    {
        protected readonly ILogger<CommonClient> _logger = logger;
        protected readonly HttpClient _httpClient = httpClient;
        protected readonly SemaphoreSlim _semaphore = semaphoreSlim;

        public CookieContainer CookieContainer { get; } = new();

        public async virtual Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                var cookieHeader = CookieContainer.GetCookieHeader(request.RequestUri!);
                if (!string.IsNullOrEmpty(cookieHeader)) request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                {
                    foreach (var cookieValue in setCookieValues)
                    {
                        try
                        {
                            CookieContainer.SetCookies(request.RequestUri!, cookieValue);
                        }
                        catch (Exception ex) when (ex is CookieException or ArgumentException)
                        {
                            if (_logger.IsEnabled(LogLevel.Warning)) _logger.LogWarning(ex, "忽略非法 Set-Cookie: {Cookie}", cookieValue);
                        }
                    }
                }
                return response;
            }
            finally { _semaphore.Release(); }
        }
    }

    public static class CommonClientExtensions
    {
        public static IServiceCollection AddCommonClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<HttpOptions>(configuration.GetSection(nameof(HttpOptions)))
                .AddSingleton<SemaphoreSlim>(sp =>
                {
                    // http 并发限制
                    var options = sp.GetRequiredService<IOptions<HttpOptions>>().Value;
                    return new SemaphoreSlim(options.MaxRequests, options.MaxRequests); 
                })
                .AddTransient<RetryHandler>()
                .AddHttpClient<ICommonClient, CommonClient>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<HttpOptions>>().Value;
                    client.Timeout = TimeSpan.FromMilliseconds(options.OverallTimeoutMilliseconds);
                })
                .ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<HttpOptions>>().Value;
                    var handler = new SocketsHttpHandler
                    {
                        MaxConnectionsPerServer = options.MaxConnectionsPerServer,
                        PooledConnectionIdleTimeout = TimeSpan.FromMilliseconds(options.PooledConnectionIdleTimeoutMilliseconds),
                        AllowAutoRedirect = false,
                        UseCookies = true,
                    };
                    return handler;
                })
                .AddHttpMessageHandler<RetryHandler>();

            return services;
        }
    }

    public class HttpOptions
    {
        public int MaxConnectionsPerServer { get; set; } = 128;
        public int PooledConnectionIdleTimeoutMilliseconds { get; set; } = 60000;
        public int OverallTimeoutMilliseconds { get; set; } = 30000;
        public int MaxRetry { get; set; } = 3;
        public int MaxRequests { get; set; } = 1024;
    }
}
