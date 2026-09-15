#pragma warning disable IDE1006 // Naming Styles

namespace GDUTSharp.Shared.Type.DTO;

public class NoticeDtoCollection
{
    public int pages { get; set; }
    
    public int size { get; set; }
    
    public int pageNo { get; set; }

    public List<NoticeDto> list { get; set; } = [];

    public static implicit operator NoticeCollection(NoticeDtoCollection source) => 
        new()
        {
            Notices = [..source.list],
            CurrentPage = source.pageNo,
            Total = source.size,
            PageCount = source.pages,
        };
}

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

    public static implicit operator Notice(NoticeDto dto)
    {
        var r = new Notice
        {
            Id = dto.id,
            Title = dto.title,
            Profile = dto.content,
            ImageUrl = dto.imageUrl,
            Publisher = dto.publishUserName,
            PublishDepart = dto.publishUserDepart,
            CreateDate = dto.createDate,
            PublishDate = dto.publishDate1,
            TypeName = dto.typeName,
            ReadCount = dto.readCount,
        };
        return r;

    }
}

#pragma warning restore IDE1006 // Naming Styles