using System.Net;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 图书馆
/// </summary>
public interface ILibraryService
{
    /// <summary>登录</summary>
    /// <returns>返回是否登录成功以及登录后获取的 JwtOpacAuth (后者在某些功能需要用到，如 <see cref="GetDailyRecommend"/>)</returns>
    public Task<(bool IsSuccess, string? JwtOpacAuth)> Login(CookieContainer cookies, LoginInfo? loginInfo = null, CancellationToken token = default);

    /// <summary>借阅列表</summary>
    public Task<List<BookInfo>?> GetBorrowedBooks(CookieContainer cookies, CancellationToken token = default);

    /// <summary>每日推荐</summary>
    /// <remarks>如果 <paramref name="jwtOpacAuth"/> 为 null，则返回与用户无关的推荐，否则是与用户相关的推荐（暂无证据表明它是基于什么推荐的）</remarks>
    public Task<BookInfo?> GetDailyRecommend(CookieContainer cookies, string? jwtOpacAuth = null, CancellationToken token = default);
}
