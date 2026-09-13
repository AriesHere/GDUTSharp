using GDUTSharp.Interfaces;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class NoticeService(ILogger<NoticeService> logger, ICommonClient client) : INoticeService
{
    protected ILogger<NoticeService> _logger = logger;
    protected ICommonClient _client = client;

    public async virtual void Preprocess()
    {
        throw new NotImplementedException();
    }

    public async virtual void GetAnnouncement()
    {
        throw new NotImplementedException();
    }

    public async virtual void GetBulletin()
    {
        throw new NotImplementedException();
    }

    public async virtual void GetNotice()
    {
        throw new NotImplementedException();
    }

    public async virtual void GetTender()
    {
        throw new NotImplementedException();
    }
}
