#pragma warning disable IDE1006 // Naming Styles

namespace GDUTSharp.Shared.Type.DTO;

public class NoticeDto
{
    public string id { get; set; } = string.Empty;

    public string subject { get; set; } = string.Empty;

    public DateTime createDate { get; set; }

    public string categoryLabel { get; set; } = string.Empty;

    public string typeId { get; set; } = string.Empty;

    public string brief { get; set; } = string.Empty;

    public int viewNum { get; set; }

    public string publishDepartment { get; set; } = string.Empty;
}

#pragma warning restore IDE1006 // Naming Styles