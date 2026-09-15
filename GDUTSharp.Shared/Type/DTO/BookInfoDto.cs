using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Type.DTO;

/// TODO: 从 https://opac.gdut.edu.cn/#/searchList/bookDetails/{recordId} 获取详细信息
/// 
/// 当前分析结果：
/// 
/// 从 https://opac.gdut.edu.cn/find/searchResultDetail/getDetail?recordId={recordId}
/// 获取图书信息（GET），其中 data -> link 是用 isbn 在各大网站中进行搜索，data -> baseMarcInfoDto -> auther
/// 是一个 html 元素，建议从 data -> baseMarcInfoDto -> cleanAuthor 而不是此处获取作者信息
/// 
/// 而馆藏信息则需要从 https://opac.gdut.edu.cn/find/physical/groupitems 中获取（POST）
/// 请求 {"page":1,"rows":10,"entrance":null,"recordId":"{recordId}","isUnify":true,"sortType":0,"callNo":""}

#pragma warning disable IDE1006 // Naming Styles

/// <remarks>
/// 原 json 摘要：
/// <code>
/// {
///     "success": true,
///     "errCode": 200,
///     "data": {
///         "title": "XXX",
///         "author": "XXX",
///         // 以下省略
///     }
/// }
/// </code>
/// </remarks>
public class DailyRecommandDtoCollection
{
    public BookInfoDto data { get; set; } = new();

    public static implicit operator BookInfo(DailyRecommandDtoCollection collection) => collection.data;
}

/// <remarks>
/// 原 json 摘要：
/// <code>
/// {
///     "success": true,
///     "errCode": 200,
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

    public static implicit operator List<BookInfo>(BorrowedBookDtoCollection? collection) => collection is null ?[] : [.. collection.data.searchResult];
}

public class BorrowedBookDtoData
{
    public List<BookInfoDto> searchResult { get; set; } = [];

    public int loanNum { get; set; } = 0;

    public int numFound { get; set; } = 0;
}

public class BookInfoDto
{
    /// <remarks>查询图书详细信息主要依靠它</remarks>
    public int recordId { get; set; } = 0;

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

    public static implicit operator BookInfo(BookInfoDto dto)
    {
        var r = new BookInfo
        {
            RecordId = dto.recordId,
            Title = dto.title,
            Author = dto.author,
            Publisher = dto.publisher,
            ISBN = dto.isbn,
            PublishYear = dto.publishYear,
            LocationName = dto.locationName,
            Barcode = dto.barcode,
            PropNo = dto.propNo,
            Index = dto.callNo,
        };
        if (!string.IsNullOrWhiteSpace(dto.loanDate)) r.LoanDate = DateOnly.Parse(dto.loanDate);
        if (!string.IsNullOrWhiteSpace(dto.normReturnDate)) r.NormReturnDate = DateOnly.Parse(dto.normReturnDate);
        return r;
    }
}

#pragma warning restore IDE1006 // Naming Styles
