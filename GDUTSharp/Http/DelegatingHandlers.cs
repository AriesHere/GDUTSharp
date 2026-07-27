using GDUTSharp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Http
{
    public partial class RetryHandler(ILogger<RetryHandler> logger, IOptions<HttpOptions> options) : DelegatingHandler
    {
        private readonly ILogger<RetryHandler> _logger = logger;
        private readonly int _maxRetry = options.Value.MaxRetry;

        /// <summary>
        /// 如果达到最大尝试次数仍未成功，则会抛出异常
        /// </summary>
        /// <exception cref="HttpRequestException"/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            await Retry(request, cancellationToken);

        /// <exception cref="HttpRequestException"/>
        private async Task<HttpResponseMessage> Retry(HttpRequestMessage request, CancellationToken cancellationToken, int attempt = 0)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (HttpRequestException e) when (attempt < _maxRetry)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求 {Uri} 第 {attempt} 次尝试时抛出异常，正在重试。{Exception}", request.RequestUri, attempt + 1, e);
                return await Retry(request, cancellationToken, attempt + 1);
            }
            catch (TaskCanceledException e) when (attempt < _maxRetry)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求 {Uri} 第 {attempt} 次尝试时超时，正在重试。{Exception}", request.RequestUri, attempt + 1, e);
                return await Retry(request, cancellationToken, attempt + 1);
            }
        }
    }

    public class ConcurrencyHandler(IOptions<HttpOptions> options) : DelegatingHandler
    {
        private readonly SemaphoreSlim _semaphore = new(options.Value.MaxRequests, options.Value.MaxRequests);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try { return await base.SendAsync(request, ct); }
            finally { _semaphore.Release(); }
        }
    }
}
