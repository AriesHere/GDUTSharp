namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

/// <remarks>
/// 原 json 摘要：
/// <code>
/// {
///     "success": true,
///     "message": "操作成功",
///     "errCode": 200,
///     "errorCode": null,
///     "data": {
///         "searchResult": [
///             {
///                 "title": "XXX",
///                 "author": "XXX",
///                 // 以下省略
///             }
///         ],
///         "loanNum": 29,
///         "numFound": 1
///     }
/// }
/// </code>
/// </remarks>
public class BorrowedBookDtoCollection
{
    public BorrowedBookDtoData data { get; set; } = new();

    public static implicit operator List<BorrowedBook>(BorrowedBookDtoCollection? collection) => collection is null ?[] : [.. collection.data.searchResult];
}

public class BorrowedBookDtoData
{
    public List<BorrowedBookDto> searchResult { get; set; } = [];

    public int loanNum { get; set; } = 0;

    public int numFound { get; set; } = 0;
}

public class BorrowedBookDto
{
    public string title { get; set; } = string.Empty;

    public string author { get; set; } = string.Empty;

    public string publisher { get; set; } = string.Empty;

    public string isbn { get; set; } = string.Empty;

    public string publishYear { get; set; } = string.Empty;

    /// <summary>借阅日期</summary>
    public string loanDate { get; set; } = string.Empty;

    /// <summary>应还日期</summary>
    public string normReturnDate { get; set; } = string.Empty;

    /// <summary>借阅馆藏地</summary>
    public string locationName { get; set; } = string.Empty;

    /// <summary>借阅图书馆</summary>
    public string phyLibName { get; set; } = string.Empty;

    /// <summary>条码号</summary>
    public string barcode { get; set; } = string.Empty;

    /// <summary>财产号</summary>
    public string propNo { get; set; } = string.Empty;

    /// <summary>索书号</summary>
    public string callNo { get; set; } = string.Empty;

    public static implicit operator BorrowedBook(BorrowedBookDto dto)
    {
        return new BorrowedBook
        {
            Title = dto.title,
            Author = dto.author,
            Publisher = dto.publisher,
            ISBN = dto.isbn,
            PublishYear = dto.publishYear,
            LoanDate = DateOnly.Parse(dto.loanDate),
            NormReturnDate = DateOnly.Parse(dto.normReturnDate),
            LocationName = dto.locationName,
            Barcode = dto.barcode,
            PropNo = dto.propNo,
            Index = dto.callNo
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
