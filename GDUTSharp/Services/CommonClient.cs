using GDUTSharp.Http;
using GDUTSharp.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Services
{
    public class CommonClient(ILogger<CommonClient> logger, IOptions<HttpOptions> options, HttpClient httpClient, ICookieService cookieService) : ICommonClient
    {
        private readonly ILogger<CommonClient> _logger = logger;
        private readonly int _maxLogLength = options.Value.MaxLogLength;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ICookieService _cookieService = cookieService;

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
            => await SendWithCookiesAsync(request, cancellationToken);
        
        private async Task<HttpResponseMessage> SendWithCookiesAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookieHeader = _cookieService.Container.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieValue in setCookieValues)
                {
                    _cookieService.Container.SetCookies(request.RequestUri!, cookieValue);
                }
            }
            await EnsureSuccessOrThrow(response);
            return response;
        }

        private async Task EnsureSuccessOrThrow(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 400)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    if (content.Length > _maxLogLength)
                    {
                        content = content[0.._maxLogLength];
                    }
                    _logger.LogError("{Method} {Url} 返回 {StatusCode}, 部分内容: {Content}",
                        response.RequestMessage?.Method,
                        response.RequestMessage?.RequestUri,
                        (int)response.StatusCode,
                        content);
                };
                throw new HttpRequestException(null, null, response.StatusCode);
            }
        }
    }

    public static class CommonClientExtensions
    {
        public static IServiceCollection AddCommonClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<HttpOptions>(configuration.GetSection(nameof(HttpOptions)));
            services.AddTransient<ConcurrencyHandler>();
            services.AddTransient<RetryHandler>();
            services.AddHttpClient<ICommonClient, CommonClient>((sp, client) =>
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
                    UseCookies = false,
                };
                return handler;
            })
            .AddHttpMessageHandler<ConcurrencyHandler>()
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
        public int MaxLogLength { get; set; } = 512;
    }
}
