using System.Text.Json.Serialization;
using GDUTSharp.Shared.Json;

namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class NoticeDtoCollection
{
    public int pages { get; set; }
    
    public int size { get; set; }

    public int total { get; set; }
    
    public int pageNo { get; set; }

    public List<NoticeDto> list { get; set; } = [];

    public List<NoticeDto> data { get; set; } = [];

    public static implicit operator NoticeCollection?(NoticeDtoCollection? source) => 
        source is null ? null : new()
        {
            Notices = [..source.list, ..source.data],
            CurrentPage = source.pageNo,
            Total = source.size == 0 ? source.size : source.total,
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

    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public DateTime? createDate { get; set; }

    /// <remarks>此数据能精确到秒，应当优先使用</remarks>
    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public DateTime? publishDate1 { get; set; }

    [JsonConverter(typeof(FlexibleDateTimeConverter))]
    public DateTime? publishDate { get; set; }

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
            PublishDate = dto.publishDate1 ?? dto.publishDate,
            TypeName = dto.typeName,
            ReadCount = dto.readCount,
        };
        return r;

    }
}

#pragma warning restore IDE1006 // Naming Styles