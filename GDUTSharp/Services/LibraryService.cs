using System.Net;
using System.Net.Http.Json;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

/// <remarks>
/// 尚未完成
/// </remarks>
public class LibraryService(ILogger<LibraryService> logger, ICommonClient client, IAuthService authService, ISecurityService security) : ILibraryService
{
    protected readonly ILogger<LibraryService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly IAuthService _authService = authService;
    protected readonly ISecurityService _security = security;
    protected string _jwtOpacAuth = string.Empty;

    public async virtual Task<bool> Login(LoginInfo? loginInfo = null)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await _authService.LoginAndAuth(IAuthService.SupportedServices.JXFW, loginInfo);
            if (response is null) return false;

            using var reader = new StreamReader(response.Content.ReadAsStream());
            reader.ReadLine();  // skip
            if (reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == true)
            {
                // 登录失败
                return false;
            }

            // 登录成功，获取 jwtOpacAuth
            var r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            int valueIndex = r.IndexOf("value=\"");
            int start = valueIndex + "value=\"".Length;
            int end = r.IndexOf('"', start);
            string refValue = WebUtility.HtmlDecode(r[start..end]);
            var rq = new HttpRequestMessage(HttpMethod.Get, refValue);
            rq.Headers.Referrer = new(GDUTConstant.LIBRARY_LOGIN);
            response = await _client.SendAsync(rq);

            r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            int valueIndex1 = r.IndexOf("name=\"refer\" value=\"");
            start = valueIndex1 + "name=\"refer\" value=\"".Length;
            end = r.IndexOf('"', start);
            string refValue1 = WebUtility.HtmlDecode(r[start..end]);
            var rq1 = new HttpRequestMessage(HttpMethod.Get, refValue1);
            response = await _client.SendAsync(rq1);

            r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            var l = response.Headers.Location?.AbsoluteUri;
            start = l?.IndexOf("jwt=") + "jwt=".Length ?? -1;
            end = l?.IndexOf("&jwtHeader") ?? -1;
            _jwtOpacAuth = l?[start..end] ?? string.Empty;

            // [尚未验证]
            // 后面如果想要正常取得数据，需要在请求头中加入
            // jwtOpacAuth: XXXXXXX
            // 并添加 Cookie: jwt=XXXXXXX 和 jwtHeader=jwtOpacAuth

            Cookie c = new("jwt", _jwtOpacAuth, null, "gdut.edu.cn");
            Cookie c1 = new("jwtHeader", "jwtOpacAuth", null, "gdut.edu.cn");
            _client.CookieContainer.Add(c);
            _client.CookieContainer.Add(c1);

            var rq2 = new HttpRequestMessage(HttpMethod.Get, l);
            response = await _client.SendAsync(rq2);
            rq2.Dispose();
            r = await response.Content.ReadAsStringAsync();
            response.Dispose();

            _client.CookieContainer.SetCookies(new Uri("https://opac.gdut.edu.cn"), "_passport_login=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=opac.gdut.edu.cn");
            return true;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证异常。 {Exception}", e);
            return false;
        }
        finally
        {
            response?.Dispose();
        }
    }

    public async virtual Task<List<BorrowedBook>?> GetBorrowedBooks()
    {
        try
        {
            var requestContent = new Dictionary<string, string>
            {
                { "page", "1" },
                { "rows", "10" },
                //{ "sort", "normReturnDate" },
                //{ "order", "asc" },
                { "searchType", "1" },
                { "searchContent", "" },
                { "sortType", "0" },
                { "startDate", "null" },
                { "endDate", "null" }
            };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.LIBRARY_LOAN_LIST, requestContent, GDUTConstant.LIBRARY_LOAN_LIST);
            request.Headers.Add("jwtOpacAuth", _jwtOpacAuth);
            request.Headers.Add("groupCode", "800555");
            request.Headers.Add("Host", "opac.gdut.edu.cn");
            using HttpResponseMessage response = await _client.SendAsync(request);
            // 待解决的问题：这里返回内容始终为
            // {"success":false,"message":"系统访问中断，请稍后再试！","errCode":9999,"errorCode":null,"data":null}
            // 无法取得数据
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ListBorrowedBook);
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求图书借阅列表异常。 {Exception}", e);
            return null;
        }
    }
}
