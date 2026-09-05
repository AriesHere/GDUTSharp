using System.Xml.Linq;

namespace GDUTSharp.Shared.Type;

public class BorrowedBook
{
    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public string PublishYear { get; set; } = string.Empty;

    /// <summary>借阅日期</summary>
    public DateOnly LoanDate { get; set; } = DateOnly.MinValue;

    /// <summary>应还日期</summary>
    public DateOnly NormReturnDate { get; set; } = DateOnly.MinValue;

    /// <summary>借阅馆藏地</summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>条码号</summary>
    public string Barcode { get; set; } = string.Empty;

    /// <summary>财产号</summary>
    public string PropNo { get; set; } = string.Empty;

    /// <summary>索书号</summary>
    public string Index { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        BorrowedBook:
          - Title:{Title}
          - Author:{Author}
          - Publisher:{Publisher}
          - ISBN:{ISBN}
          - PublishYear:{PublishYear}
          - LoanDate:{LoanDate}
          - NormReturnDate:{NormReturnDate}
          - LocationName:{LocationName}
          - Barcode:{Barcode}
          - PropNo:{PropNo}
          - Index:{Index}
        """;
    }
}
