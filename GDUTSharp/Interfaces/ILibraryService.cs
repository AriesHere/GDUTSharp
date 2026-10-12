using System.Net;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>图书馆</summary>
/// <remarks>
/// 登录方式：调用 <see cref="Login"/> 通过统一认证中心登录，如果触发验证码验证，则必定登录失败，返回 false。
/// 此时需要通过 <see cref="IAuthService.SubmitCaptcha"/> 进行验证码验证后再调用 <see cref="Login"/>，
/// 详见 <see cref="IAuthService"/> 的文档。
/// </remarks>
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
