using System.Collections;

namespace GDUTSharp.Shared.Type;

public class NoticeCollection : IEnumerable<Notice>
{
    public int PageCount { get; set; }

    public int Total { get; set; }

    public int CurrentPage { get; set; }

    public List<Notice> Notices { get; set; } = [];

    public Notice this[int index] => this.Notices[index];

    public int Count => this.Notices.Count;

    public IEnumerator<Notice> GetEnumerator() => this.Notices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

/// <remarks>
/// 推荐使用 <see cref="NoticeCollection"/> 而不是 <see cref="List{T}"/>，因为前者能提供页信息
/// </remarks>
[Attributes.OverrideToString]
public partial class Notice
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;

    /// <remarks>需要通过 <see cref="Interfaces.INoticeService.GetNoticeImageUrl(string)"/> 拼接</remarks>
    public string ImageUrl { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string PublishDepart { get; set; } = string.Empty;

    public DateTime? CreateDate { get; set; }

    public DateTime? PublishDate { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public int ReadCount { get; set; }

    public string GetNoticeUrl() => GSConst.NoticeDetail + this.Id;

    public string GetImageUrl() => GSConst.NoticeBase + this.ImageUrl;
}
