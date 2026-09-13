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
    /// 由于通知公文网的特殊性，无法单独获取下面这四种信息，
    /// 必须先进行预处理，同时获得它们四个的链接
    /// </remarks>
    public Task<bool> Preprocess();

    /// <summary>最新通知</summary>
    /// <remarks>大部分类似于“关于xxx的通知”</remarks>
    public void GetNotice();

    /// <summary>最新简讯</summary>
    /// <remarks>大部分是宣传性文章</remarks>
    public void GetBulletin();

    /// <summary>最新公告</summary>
    /// <remarks>大部分是“xxx情况公布”或“xxx公示”</remarks>
    public void GetAnnouncement();

    /// <summary>招标公告</summary>
    public void GetTender();
}
