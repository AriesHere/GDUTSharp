using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 图书馆
/// </summary>
public interface ILibraryService
{
    public Task<bool> Login(LoginInfo? loginInfo = null);

    /// <summary>获取借阅列表</summary>
    /// <remarks>暂时不可用，详见 <see cref="GDUTSharp.Services.LibraryService.GetBorrowedBooks()"/> 的注释</remarks>
    public Task<List<BorrowedBook>?> GetBorrowedBooks();
}
