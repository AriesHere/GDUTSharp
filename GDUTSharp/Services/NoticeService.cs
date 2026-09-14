using System.Net;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class NoticeService(ILogger<NoticeService> logger, ICommonClient client) : INoticeService
{
    protected record struct NoticeArgs(string EntityId = "", string PanelId = "");

    protected ILogger<NoticeService> _logger = logger;
    protected ICommonClient _client = client;

    #region 请求数据时的相关参数

    protected string _spaceId = string.Empty;
    protected string _ownerId = string.Empty;

    /// <summary>最新通知参数</summary>
    protected NoticeArgs _noticeArgs = new();
    /// <summary>最新简讯参数</summary>
    protected NoticeArgs _bulletinArgs = new();
    /// <summary>最新公告参数</summary>
    protected NoticeArgs _announcementArgs = new();
    /// <summary>招标公告参数</summary>
    protected NoticeArgs _tenderArgs = new();

    protected virtual string GenUrl(INoticeService.NoticeType noticeType)
    {
        NoticeArgs args = noticeType switch
        {
            INoticeService.NoticeType.Bulletin => _bulletinArgs,
            INoticeService.NoticeType.Announcement => _announcementArgs,
            INoticeService.NoticeType.Tender => _tenderArgs,
            _ or INoticeService.NoticeType.Notice => _noticeArgs,
        };
        if (string.IsNullOrWhiteSpace(args.EntityId)
            || string.IsNullOrWhiteSpace(args.PanelId)
            || string.IsNullOrWhiteSpace(_spaceId)
            || string.IsNullOrWhiteSpace(_ownerId))
        {
            throw new ArgumentException($"尝试生成 url 时缺少参数。生成类型:{noticeType} SpaceId:{_spaceId} OwnerId:{_ownerId} PanelId:{args.PanelId} EntityId:{args.EntityId}");
        }
        long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        // TODO: 这是基于"最新通知"板块的参数修改的，可以考虑添加更细致的调整
        string urlArgs = WebUtility.UrlEncode($$"""{"x":"0","y":"2","xIndex":"0","sectionBeanId":"newsSection","entityId":"{{args.EntityId}}","ordinal":"0","r_ordinal":"0","fadd":"-1","spaceId":"{{_spaceId}}","spaceType":"before_login","width":"5","ownerId":"{{_ownerId}}","sprint":"","sectionWidth":569,"sbt":"0","sst":"1","b_t":"","s_tsc":"","s_tbc":"","b_s":"default","paramKeys":["lineHeight","aiSort","aiSortValue","setAiSort"],"paramValues":["35","0","0","0"],"panelId":"{{args.PanelId}}","bgc":"","bodyHeight":"825","rf":"multiRowThreeColumnTemplete","aiSort":"0","aiSortValue":"0"}""");
        return string.Format(GDUTConstant.NOTICE_GET_PREFIX, time, urlArgs);
    }

    #endregion

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
            var span = r.AsSpan(r.IndexOf("公文悬浮菜单js"));
            position = span.IndexOf("/seeyon/main.do?");
            url = span[position..span.IndexOf('\"')].ToString();

            request = new(HttpMethod.Get, url);
            response = await _client.SendAsync(request);
            request.Dispose();
            r = await response.Content.ReadAsStringAsync();
            span = r.AsSpan(r.IndexOf("\"spaceId\""));

            position = span.IndexOf("\": \"") + "\": \"".Length;
            span = span[position..];
            _spaceId = span[position..span.IndexOf('\"')].ToString();

            position = span.IndexOf("\"ownerId\": \"") + "\"ownerId\": \"".Length;
            span = span[position..];
            _ownerId = span[position..span.IndexOf('\"')].ToString();

            // 获取各板块参数
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _tenderArgs.EntityId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _tenderArgs.PanelId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _noticeArgs.EntityId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _noticeArgs.PanelId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _bulletinArgs.EntityId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _bulletinArgs.PanelId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _announcementArgs.EntityId = span[position..span.IndexOf('\"')].ToString();
            position = span.IndexOf("\"id\": \"") + "\"id\": \"".Length;
            span = span[position..];
            _announcementArgs.PanelId = span[position..span.IndexOf('\"')].ToString();
            // (0,0) => 图片横幅
            // (0,1) => 公告查询
            // (1,1) => 招标公告
            // (0,2) => 最新通知
            // (0,3) => 最新简讯
            // (1,3) => 最新公告
            return true;
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

    public async virtual void GetNotice(INoticeService.NoticeType noticeType)
    {
        throw new NotImplementedException();
    }
}
