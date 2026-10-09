using GDUTSharp.Services;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Http;

public partial class RetryHandler(ILogger<RetryHandler> logger, IOptions<HttpOptions> options) : DelegatingHandler
{
    private readonly ILogger<RetryHandler> _logger = logger;
    private readonly int _maxRetry = options.Value.MaxRetry;
    private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(options.Value.RetryDelay);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is not null) await request.Content.LoadIntoBufferAsync(cancellationToken);
        Exception? lastException = null;
        for (int attempt = 1; attempt <= _maxRetry; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            HttpRequestMessage attemptRequest = attempt == 1 ? request : await CloneRequestAsync(request, cancellationToken);
            try
            {
                return await base.SendAsync(attemptRequest, cancellationToken);
            }
            catch (HttpRequestException e) when (!cancellationToken.IsCancellationRequested)
            {
                Log.HttpRequestError(_logger, request.RequestUri, attempt, e);
                lastException = e;
            }
            catch (OperationCanceledException e) when (!cancellationToken.IsCancellationRequested)
            {
                Log.HttpRequestTimeout(_logger, request.RequestUri, attempt, e);
                lastException = e;
            }
            finally
            {
                if (!ReferenceEquals(attemptRequest, request)) attemptRequest.Dispose();
            }
            if (attempt < _maxRetry) await Task.Delay(_delay, cancellationToken);
            _logger.LogTrace("aa{0}", attempt);
        }
        throw new HttpRequestException($"请求 {request.RequestUri} 达到最大尝试次数 {_maxRetry}，仍未成功。", lastException);
    }

    /// <summary>
    /// 克隆一个 HttpRequestMessage，内容用已缓冲好的副本重建。
    /// </summary>
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage source, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri)
        {
            Version = source.Version,
            VersionPolicy = source.VersionPolicy,
        };
        if (source.Content is not null)
        {
            var buffer = await source.Content.ReadAsByteArrayAsync(ct);
            var content = new ByteArrayContent(buffer);
            foreach (var header in source.Content.Headers) content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            clone.Content = content;
        }
        foreach (var header in source.Headers) clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        foreach (var kv in source.Options) clone.Options.Set(new HttpRequestOptionsKey<object?>(kv.Key), kv.Value);
        return clone;
    }
}
