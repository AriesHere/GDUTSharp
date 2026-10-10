using GDUTSharp.Shared.Type;
using Ical.Net.CalendarComponents;

namespace GDUTSharp.Extra.Types;

public sealed class ICalConvertOptions<T>
{
    public SessionCollection Sessions = SessionCollection.Default;

    /// <summary>是否在 <see cref="Sessions"/> 连续时自动合并时间</summary>
    public bool IsMergeIfContinuous = true;

    /// <summary>首周星期一的日期</summary>
    /// <remarks>如果为 null，请确保数据中的 Date 一定为有效值</remarks>
    public DateOnly? StartDate = null;

    public List<string> Categories = [];

    /// <summary>
    /// 通过传入的类来设置提醒，如果返回 null，则使用默认提醒（即 <see cref="Alarm"/>）
    /// </summary>
    public event Func<T, Alarm?>? SetAlarmFunc;

    /// <summary>默认提醒</summary>
    public Alarm? Alarm = null;

    public Alarm? SetAlarm(T source) => SetAlarmFunc?.Invoke(source) ?? Alarm;
}
