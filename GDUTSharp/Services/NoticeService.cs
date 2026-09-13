using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class NoticeService(ILogger<NoticeService> logger, ICommonClient client) : INoticeService
{
    protected ILogger<NoticeService> _logger = logger;
    protected ICommonClient _client = client;

    public async virtual Task<bool> Preprocess()
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = new(HttpMethod.Get, GDUTConstant.NOTICE_BEFORE_LOGIN);
            response = await _client.SendAsync(request);
            request.Dispose();
            var r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            var position = r.IndexOf('?');
            var url = r[position..r.IndexOf('\"', position)];

            request = new(HttpMethod.Get, GDUTConstant.NOTICE_BEFORE_LOGIN + url);
            response = await _client.SendAsync(request);
            request.Dispose();
            r = await response.Content.ReadAsStringAsync(); // 返回内容很大
            var p1 = r.IndexOf("公文悬浮菜单js");
            var span = r.AsSpan(p1);
            position = span.IndexOf("/seeyon/main.do?");
            url = span[position..span.IndexOf('\"')].ToString();

            request = new(HttpMethod.Get, url);
            response = await _client.SendAsync(request);
            request.Dispose();
            r = await response.Content.ReadAsStringAsync();

            // (0,0) => 图片横幅
            // (0,1) => 公告查询
            // (1,1) => 招标公告
            // (0,2) => 最新通知
            // (0,3) => 最新简讯
            // (1,3) => 最新公告
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证或登录异常。 {Exception}", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
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
