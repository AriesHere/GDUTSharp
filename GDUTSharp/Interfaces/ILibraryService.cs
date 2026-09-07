using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 图书馆
/// </summary>
public interface ILibraryService
{
    public Task<bool> Login(LoginInfo? loginInfo = null);

    public Task<List<BorrowedBook>?> GetBorrowedBooks();
}
