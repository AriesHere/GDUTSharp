using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class LibraryService(ILogger<LibraryService> logger, ICommonClient client, IAuthService authService) : ILibraryService
{
    protected readonly ILogger<LibraryService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly IAuthService _authService = authService;
    /// <remarks>
    /// 某些请求需要在请求头添加 HeaderName: "jwtOpacAuth" HeaderValue: _jwtOpacAuth
    /// 如<see cref="GetDailyRecommand"/>
    /// </remarks>
    protected string _jwtOpacAuth = string.Empty;

    public async virtual Task<bool> Login(LoginInfo? loginInfo = null)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            response = await _authService.LoginAndAuth(IAuthService.SupportedServices.LIBRARY, loginInfo);
            if (response is null) return false;

            using var reader = new StreamReader(response.Content.ReadAsStream());
            reader.ReadLine();  // skip
            if (reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == true)
            {
                // 登录失败
                return false;
            }

            var r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            string refValue = WebUtility.HtmlDecode(r.Extract("value=\"", '"', out _));
            request = new(HttpMethod.Get, refValue);
            request.Headers.Referrer = new(GSConst.LIBRARY_LOGIN);
            response = await _client.SendAsync(request);
            request.Dispose();

            r = await response.Content.ReadAsStringAsync();
            response.Dispose();
            int valueIndex1 = r.IndexOf("name=\"refer\" value=\"");
            string refValue1 = WebUtility.HtmlDecode(r.Extract("name=\"refer\" value=\"", '"', out _));
            request = new(HttpMethod.Get, refValue1);
            response = await _client.SendAsync(request);
            request.Dispose();

            r = response.RequestMessage?.RequestUri?.AbsoluteUri;
            response.Dispose();
            Exception.ThrowIfNull(r, "获取 jwtOpacAuth 失败。");

            _client.CookieContainer.Add(new Cookie("jwt", r!.Extract("jwt=", "&jwtHeader", out _), null, "gdut.edu.cn"));
            _client.CookieContainer.Add(new Cookie("jwtHeader", "jwtOpacAuth", null, "gdut.edu.cn"));
            return true;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证异常。 {Exception}", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    public async virtual Task<List<BookInfo>?> GetBorrowedBooks()
    {
        try
        {
            var requestContent = """
                {
                    "page": 1,
                    "rows": 10,
                    "searchType": 1,
                    "searchContent": "",
                    "sortType": 0,
                    "startDate": null,
                    "endDate": null
                }
                """;
            using var request = new HttpRequestMessage(HttpMethod.Post, GSConst.LIBRARY_LOAN_LIST)
            {
                Content = new StringContent(requestContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
            };
            using HttpResponseMessage response = await _client.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.BorrowedBookDtoCollection);
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求图书借阅列表异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<BookInfo?> GetDailyRecommend()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.LIBRARY_DAILY_RECOMMEND);
            request.Headers.Referrer = new(GSConst.LIBRARY_DAILY_RECOMMEND);
            request.Headers.Add("jwtOpacAuth", _jwtOpacAuth);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.DailyRecommandDtoCollection);
            if (result is null) return null;
            else return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求图书馆每日推荐异常。 {Exception}", e);
            return null;
        }
    }
}
