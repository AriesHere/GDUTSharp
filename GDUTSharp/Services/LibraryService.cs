using System.Diagnostics;
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
public class LibraryService(ILogger<LibraryService> logger, ICommonClient client) : ILibraryService
{
    protected readonly ILogger<LibraryService> _logger = logger;
    protected readonly ICommonClient _client = client;

    public Task<bool> Login(LoginInfo? loginInfo = null)
    {
        throw new NotImplementedException();
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
