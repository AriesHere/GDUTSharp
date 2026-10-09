using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public partial class NoticeService(ILogger<NoticeService> logger, ICommonClient client) : INoticeService
{
    protected ILogger<NoticeService> _logger = logger;
    protected ICommonClient _client = client;

    /// <summary>生成用于请求分类通知的 <see cref="HttpRequestMessage"/></summary>
    /// <remarks><paramref name="id"/> 通过 <see cref="Preprocess"/> 获取的 Dictionary 得到</remarks>
    /// <param name="id">通过 <see cref="Preprocess"/> 获取的 Dictionary 得到</param>
    protected virtual HttpRequestMessage CreateRequest(string id, int pageNumber, int pageSize)
    {
        var content = new Dictionary<string, string> {
            { "managerMethod", "findListDatas" },
            { "arguments", $$"""
                [{
                    "pageSize":"{{pageSize}}",
                    "pageNo":{{pageNumber}},
                    "listType":"1",
                    "spaceType":"2",
                    "spaceId":"",
                    "typeId":"",
                    "condition":"publishDepartment",
                    "textfield1":"",
                    "textfield2":"",
                    "myNews":"",
                    "fragmentId":"{{id}}",
                    "ordinal":"0",
                    "panelValue":"designated_value"
                }]
                """ },
        };
        return ICommonClient.CreateRequest(HttpMethod.Post, GSConst.NOTICE_CATEGORIES_GET, content);
    }

    public virtual async Task<(Dictionary<string, string> MainCategories, Dictionary<string, string> SubCategories)?> Preprocess(
        CookieContainer cookies,
        CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = new(HttpMethod.Get, GSConst.NOTICE_BEFORE_LOGIN);
            response = await _client.SendAsync(cookies, request, token);
            request.Dispose();
            response.Dispose();

            request = new(HttpMethod.Get, GSConst.NOTICE_CATEGORIES);
            response = await _client.SendAsync(cookies, request, token);
            request.Dispose();
            var r = await response.Content.ReadAsStringAsync(token);
            response.Dispose();

            var matches = Helper.Notice_CategoriesRegex().Matches(r);
            if (matches.Count <= 4)
            {
                throw new ArgumentException("正则匹配失败");
            }
            Dictionary<string, string> mainCategories = [];
            Dictionary<string, string> subCategories = [];
            for (int i = 0; i < matches.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(matches[i].Groups["value"].Value))
                    continue;
                (i >= 2 ? mainCategories : subCategories).Add(matches[i].Groups["text"].Value, matches[i].Groups["value"].Value);
            }

            return (mainCategories, subCategories);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "预处理异常。 {Exception}", e);
            return null;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }
    
    public virtual async Task<NoticeCollection?> GetNoticeCollection(CookieContainer cookies, string id, int pageNumber, int pageSize, CancellationToken token = default)
    {
        try
        {
            using HttpRequestMessage request = this.CreateRequest(id, pageNumber, pageSize);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.NoticeDtoCollection, token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取通知数据时抛出异常。 {Exception}", e);
            return null;
        }
    }
}
