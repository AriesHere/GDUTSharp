using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 图书馆
/// </summary>
public interface ILibraryService
{
    public Task<List<BorrowedBook>?> GetBorrowedBooks();
}
