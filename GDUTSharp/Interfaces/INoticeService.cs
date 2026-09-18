using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 通知简讯网
/// </summary>
/// <remarks>
/// 首次使用时需要先调用 <see cref="Preprocess"/> 才能再调用其它的
/// </remarks>
public interface INoticeService
{
    /// <summary>预处理</summary>
    /// <remarks>
    /// 由于通知公文网的特殊性，无法单独获取各分类信息
    /// 必须先进行预处理，获得各分类的 id
    /// </remarks>
    public Task<(Dictionary<string, string> MainCategories, Dictionary<string, string> SubCategories)?> Preprocess();

    /// <summary>
    /// 通过分类的 id 来获取对应分类的 NoticeCollection
    /// </summary>
    public Task<NoticeCollection?> GetNoticeCollection(string id, int pageNumber, int pageSize = 20);

    /// <summary>拼接通知详情的 url</summary>
    public static string GetNoticeDetailUrl(string id) => string.Format(GDUTConstant.NOTICE_DETAIL, id);

    /// <summary>拼接通知图片的 url</summary>
    /// <remarks><paramref name="imageUrl"/> 是 <see cref="Notice.ImageUrl"/></remarks>
    public static string GetNoticeImageUrl(string imageUrl) => GDUTConstant.NOTICE_BASE + imageUrl;
}
