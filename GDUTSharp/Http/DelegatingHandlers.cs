using GDUTSharp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Http;

/// <summary>
/// 自动重试
/// </summary>
public partial class RetryHandler(ILogger<RetryHandler> logger, IOptions<HttpOptions> options) : DelegatingHandler
{
    private readonly ILogger<RetryHandler> _logger = logger;
    private readonly int _maxRetry = options.Value.MaxRetry;    
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(options.Value.RetryDelay);

    /// <summary>
    /// 如果达到最大尝试次数仍未成功，则会抛出异常
    /// </summary>
    /// <exception cref="HttpRequestException"/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        Retry(request, cancellationToken);

    /// <exception cref="HttpRequestException">达到最大尝试次数仍未成功</exception>
    /// <exception cref="OperationCanceledException">调用方主动取消</exception>
    private async Task<HttpResponseMessage> Retry(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        for (int attempt = 1; attempt <= _maxRetry; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (HttpRequestException e) when (!cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求 {uri} 第 {attempt} 次尝试抛出异常。{exception}", request.RequestUri, attempt, e);
                lastException = e;
            }
            catch (OperationCanceledException e) when (!cancellationToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求 {uri} 第 {attempt} 次尝试超时。{exception}", request.RequestUri, attempt, e);
                lastException = e;
            }
            if (attempt < _maxRetry)
            {
                await Task.Delay(_delay, cancellationToken);
            }
        }
        throw new HttpRequestException($"请求 {request.RequestUri} 达到最大尝试次数 {_maxRetry}，仍未成功。", lastException);
    }
}
