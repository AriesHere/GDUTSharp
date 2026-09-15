using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public partial class NoticeService(ILogger<NoticeService> logger, ICommonClient client) : INoticeService
{
    protected ILogger<NoticeService> _logger = logger;
    protected ICommonClient _client = client;

    #region 请求数据时的相关参数

    protected string _spaceId = string.Empty;
    protected string _ownerId = string.Empty;

    public Dictionary<string, string> MainCategories => [];

    public Dictionary<string, string> SubCategories => [];

    /// <remarks>
    /// <paramref name="id"/> 请通过 <see cref="MainCategories"/> 或 <see cref="SubCategories"/> 获取
    /// </remarks>
    protected virtual HttpRequestMessage GenRequest(string id, int pageNumber = 1, int pageSize = 20)
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
        return ICommonClient.CreateRequest(HttpMethod.Post, string.Format(GDUTConstant.NOTICE_GET, id), content);
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
            response.Dispose();

            request = new(HttpMethod.Get, GDUTConstant.NOTICE_CATEGORIES);
            response = await _client.SendAsync(request);
            request.Dispose();
            var r = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var matches = Helper.Notice_CategoriesRegex().Matches(r);
            var matchResults = new List<(string Value, string Key)>();
            if (matchResults.Count <= 2)
            {
                throw new ArgumentException("正则匹配失败");
            }
            int flag = 0;
            for (int i = 0; i < matchResults.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(matchResults[i].Value))
                {
                    flag++;
                    continue;
                }
                (flag >= 2 ? MainCategories : SubCategories).Add(matchResults[i].Key, matchResults[i].Value);
            }

            return true;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("预处理异常。 {Exception}", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    public void GetNotice(string category)
    {
        throw new NotImplementedException();
    }

    public void FetchCategories()
    {
        throw new NotImplementedException();
    }
}
