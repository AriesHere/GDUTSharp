using GDUTSharp.Shared.Type;
using Ical.Net.CalendarComponents;

namespace GDUTSharp.Extra.Types;

public class ICalConvertOptions
{
    public SessionCollection Sessions = SessionCollection.Default;

    /// <summary>是否在 <see cref="Sessions"/> 连续时自动合并时间</summary>
    public bool IsMergeIfContinuous = true;

    /// <summary>首周星期一的日期</summary>
    /// <remarks>如果为 null，请确保数据中的 Date 一定为有效值</remarks>
    public DateOnly? StartDate = null;

    public List<string> Categories = [];

    /// <summary>
    /// 通过输入 Lesson 设置提醒，如果返回 null，则使用默认提醒（即 <see cref="Alarm"/>）
    /// </summary>
    /// <remarks>
    /// 仅对 <see cref="Lesson"/> 有效，其它类型只使用默认提醒（即 <see cref="Alarm"/>）
    /// </remarks>
    public event Func<Lesson, Alarm?>? SetAlarmFunc;

    /// <summary>默认提醒</summary>
    public Alarm? Alarm = null;

    /// <remarks>
    /// 仅对 <see cref="Lesson"/> 有效，其它类型只使用默认提醒（即 <see cref="Alarm"/>）
    /// </remarks>
    public Alarm? SetAlarm(Lesson lesson) => SetAlarmFunc?.Invoke(lesson) ?? Alarm;
}
