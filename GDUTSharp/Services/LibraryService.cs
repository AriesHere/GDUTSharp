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

    public virtual async Task<(bool IsSuccess, string? JwtOpacAuth)> Login(
        CookieContainer cookies,
        LoginInfo? loginInfo = null,
        CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            response = await _authService.LoginAndAuth(cookies, IAuthService.SupportedServices.LIBRARY, loginInfo, token);
            if (response is null) return (false, null);

            using var reader = new StreamReader(response.Content.ReadAsStream(token));
            reader.ReadLine();  // skip
            if (reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == true)
            {
                Log.LoginFailed(_logger, null);
                return (false, null);
            }

            var r = await response.Content.ReadAsStringAsync(token);
            response.Dispose();
            string refValue = WebUtility.HtmlDecode(r.Extract("value=\"", '"', out _));
            request = new(HttpMethod.Get, refValue);
            request.Headers.Referrer = new(GSConst.LIBRARY_LOGIN);
            response = await _client.SendAsync(cookies, request, token);
            request.Dispose();

            r = await response.Content.ReadAsStringAsync(token);
            response.Dispose();
            refValue = WebUtility.HtmlDecode(r.Extract("name=\"refer\" value=\"", '"', out _));
            request = new(HttpMethod.Get, refValue);
            response = await _client.SendAsync(cookies, request, token);
            request.Dispose();

            r = response.RequestMessage?.RequestUri?.AbsoluteUri;
            response.Dispose();
            ArgumentException.ThrowIfNullOrEmpty(r);

            cookies.Add(new Cookie("jwt", r!.Extract("jwt=", "&jwtHeader", out _), null, "gdut.edu.cn"));
            cookies.Add(new Cookie("jwtHeader", "jwtOpacAuth", null, "gdut.edu.cn"));
            return (true, r);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "图书馆登录", e);
            return (false, null);
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    public virtual async Task<List<BookInfo>?> GetBorrowedBooks(CookieContainer cookies, CancellationToken token = default)
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
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.BorrowedBookDtoCollection, token);
            return result;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求图书借阅列表", e);
            return null;
        }
    }

    public virtual async Task<BookInfo?> GetDailyRecommend(CookieContainer cookies, string? jwtOpacAuth = null, CancellationToken token = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.LIBRARY_DAILY_RECOMMEND);
            request.Headers.Referrer = new(GSConst.LIBRARY_DAILY_RECOMMEND);
            if (jwtOpacAuth is not null) request.Headers.Add("jwtOpacAuth", jwtOpacAuth);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.DailyRecommandDtoCollection, token);
            if (result is null) return null;
            else return result;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求图书馆每日推荐", e);
            return null;
        }
    }
}
