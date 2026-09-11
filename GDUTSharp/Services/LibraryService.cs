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

    public async virtual Task<bool> Login(LoginInfo? loginInfo = null)
    {
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
            await _client.SendAsync(rq1);   // 无需知道其内容

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
            using var request = new HttpRequestMessage(HttpMethod.Post, GDUTConstant.LIBRARY_LOAN_LIST)
            {
                Content = new StringContent(requestContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json")),
            };
            using HttpResponseMessage response = await _client.SendAsync(request);
            _logger.LogCritical("{0}", await response.Content.ReadAsStringAsync());
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.BorrowedBookDtoCollection);
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求图书借阅列表异常。 {Exception}", e);
            return null;
        }
    }
}
