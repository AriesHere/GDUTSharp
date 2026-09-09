using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

/// <remarks>
/// 尚未完成
/// </remarks>
public class LibraryService(ILogger<LibraryService> logger, ICommonClient client, ISecurityService security) : ILibraryService
{
    protected readonly ILogger<LibraryService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly ISecurityService _security = security;
    protected string _jwtOpacAuth = string.Empty;

    public async Task<bool> Login(LoginInfo? loginInfo = null)
    {
        HttpResponseMessage? response = null;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, GDUTConstant.AUTHSERVER_AUTH_Prefix + GDUTConstant.LIBRARY_LOGIN);
            response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // 需要登录
                if (loginInfo is null)
                {
                    return false;
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("正在登录并认证");
                    var formData = new Dictionary<string, string>();
                    string pwdEncryptSalt = string.Empty;
                    string html = await response.Content.ReadAsStringAsync();
                    response.Dispose();

                    // 这里采用了相当激进的优化，如果校方改东西了，可能会出错。如果不希望这
                    // 样，请使用 GDUTSharp.Extra.SteadyDataService 中的 Login 方法
                    Match saltMatch = Helper.Login_SaltRegex().Match(html);
                    pwdEncryptSalt = saltMatch.Success ? saltMatch.Groups[1].Value : "";
                    Match execMatch = Helper.Login_ExecRegex().Match(html);
                    string execution = execMatch.Success ? execMatch.Groups[1].Value : "";
                    formData["_eventId"] = "submit";
                    formData["cllt"] = "userNameLogin";
                    formData["dllt"] = "generalLogin";
                    formData["lt"] = "";
                    formData["execution"] = execution;
                    formData[""] = pwdEncryptSalt;
                    formData["username"] = loginInfo.UserName;
                    formData["password"] = _security.CbcEncrypt(loginInfo.Password, pwdEncryptSalt);

                    using var request2 = ICommonClient.CreateRequest(
                        HttpMethod.Post,
                        GDUTConstant.AUTHSERVER_AUTH_Prefix + GDUTConstant.LIBRARY_LOGIN,
                        formData,
                        GDUTConstant.LIBRARY_LOGIN);
                    response = await _client.SendAsync(request2);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("正在认证");
            }

            for (int i = 0; i < 5; i++)
            {
                if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.MovedPermanently)
                    break;
                string? location = response.Headers.Location?.AbsoluteUri;
                if (string.IsNullOrEmpty(location))
                    break;
                if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", i + 1, location);
                response.Dispose();
                using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                response = await _client.SendAsync(redirectRequest);
            }

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
            var l = response.Headers.Location?.AbsoluteUri;
            start = l?.IndexOf("jwt=") + "jwt=".Length ?? -1;
            end = l?.IndexOf("&jwtHeader") ?? -1;
            string _jwtOpacAuth = l?[start..end] ?? string.Empty;
            // [尚未验证]
            // 后面如果想要正常取得数据，需要在请求头中加入
            // jwtOpacAuth: XXXXXXX
            // 并添加 Cookie: jwt=XXXXXXX 和 jwtHeader=jwtOpacAuth

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
            using HttpResponseMessage response = await _client.SendAsync(request);
            Debug.WriteLine(await response.Content.ReadAsStringAsync());
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
