using GDUTSharp.Services;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Http;

/// <summary>HTTP 并发限制</summary>
public class HttpConcurrencyLimiter(IOptions<HttpOptions> options) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(options.Value.MaxRequests, options.Value.MaxRequests);

    public void Dispose()
    {
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task WaitAsync(CancellationToken ct) => _semaphore.WaitAsync(ct);
    public void Release() => _semaphore.Release();
}
