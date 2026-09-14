#pragma warning disable IDE1006 // Naming Styles

namespace GDUTSharp.Shared.Type.DTO;

public class NoticeDto
{
    public string id { get; set; } = string.Empty;

    public string title { get; set; } = string.Empty;

    public string content { get; set; } = string.Empty;

    public string imageUrl { get; set; } = string.Empty;

    public string publishUserName { get; set; } = string.Empty;

    public string publishUserDepart { get; set; } = string.Empty;

    public DateTime createDate { get; set; }

    /// <remarks>原始数据中有 publishDate 而这里使用 publishDate1 是因为后者能精确到秒</remarks>
    public DateTime publishDate1 { get; set; }

    public string typeName { get; set; } = string.Empty;

    public int readCount { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles