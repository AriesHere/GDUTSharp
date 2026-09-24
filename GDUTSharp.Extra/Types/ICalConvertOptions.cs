using GDUTSharp.Shared.Type;
using Ical.Net.CalendarComponents;

namespace GDUTSharp.Extra.Types;

public class ICalConvertOptions
{
    public SessionCollection Sessions = SessionCollection.Default;

    /// <summary>是否在 <see cref="Sessions"/> 连续时自动合并时间</summary>
    public bool IsMergeIfContinuous = true;

    public Alarm? Alarm = null;

    /// <summary>首周星期一的日期</summary>
    /// <remarks>如果为 null，请确保数据中的 Date 一定为有效值</remarks>
    public DateOnly? StartDate = null;

    public List<string> Categories = [];
}
