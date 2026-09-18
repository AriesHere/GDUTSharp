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

    public async virtual Task<(Dictionary<string, string> MainCategories, Dictionary<string, string> SubCategories)?> Preprocess()
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = new(HttpMethod.Get, GDUTConstant.NOTICE_BEFORE_LOGIN);
            response = await _client.SendAsync(request);
            request.Dispose();
            response.Dispose();

            request = new(HttpMethod.Get, GDUTConstant.NOTICE_CATEGORIES);
            response = await _client.SendAsync(request);
            request.Dispose();
            var r = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var matches = Helper.Notice_CategoriesRegex().Matches(r);
            if (matches.Count <= 4)
            {
                throw new ArgumentException("正则匹配失败");
            }
            int flag = 0;
            Dictionary<string, string> mainCategories = [];
            Dictionary<string, string> subCategories = [];
            foreach (Match m in matches)
            {
                if (string.IsNullOrWhiteSpace(m.Groups["value"].Value))
                {
                    flag++;
                    continue;
                }
                (flag >= 2 ? mainCategories : subCategories).Add(m.Groups["text"].Value, m.Groups["value"].Value);
            }

            return (mainCategories, subCategories);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("预处理异常。 {Exception}", e);
            return null;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }
    
    public async Task<NoticeCollection?> GetNoticeCollection(string id, int pageNumber, int pageSize)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = this.CreateRequest(id, pageNumber, pageSize);
            response = await _client.SendAsync(request);
            request.Dispose();
            var r = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.NoticeDtoCollection);
            response.Dispose();
            if (r is null) return null;
            else return r;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("获取通知数据时抛出异常。 {Exception}", e);
            return null;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    /// <summary>生成用于请求分类通知的 <see cref="HttpRequestMessage"/></summary>
    /// <remarks>
    /// <paramref name="id"/> 通过 <see cref="Preprocess"/> 获取的 Dictionary 得到
    /// </remarks>
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
        return ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.NOTICE_CATEGORIES_GET, content);
    }

}
