using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 图书馆
/// </summary>
public interface ILibraryService
{
    public Task<bool> Login(LoginInfo? loginInfo = null);

    /// <summary>借阅列表</summary>
    public Task<List<BookInfo>?> GetBorrowedBooks();

    /// <summary>每日推荐</summary>
    public Task<BookInfo?> GetDailyRecommend();
}
