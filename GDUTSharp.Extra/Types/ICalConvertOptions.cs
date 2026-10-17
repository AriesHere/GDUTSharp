using GDUTSharp.Shared.Type;
using Ical.Net.CalendarComponents;

namespace GDUTSharp.Extra.Types;

// TODO: 分类，自定义内容
public sealed class ICalConvertOptions<T>
{
    public SessionCollection Sessions = SessionCollection.Default;

    /// <summary>是否在 <see cref="Sessions"/> 连续时自动合并时间</summary>
    public bool IsMergeIfContinuous = true;

    /// <summary>首周星期一的日期</summary>
    /// <remarks>如果为 null，请确保数据中的 Date 一定为有效值</remarks>
    public DateOnly? StartDate = null;

    public List<string> Categories = [];

    #region Description

    /// <summary>设置 <see cref="RecurringComponent.Description"/></summary>
    public Func<T, string> GetDescriptionFunc { get; set; } = GetDescription;

    /// <summary><see cref="GetDescriptionFunc"/> 的默认实现</summary>
    public static string GetDescription(T item) => item switch
    {
        Lesson lesson => lesson.Profile,
        ExamSchedule schedule => $"""
            课程名称：{schedule.Name}
            校区：{schedule.Campus}
            地点：{schedule.Location}
            监考老师：{schedule.Teachers}
            考试类别：{schedule.ExamType}
            安排类型：{schedule.ScheduleType}
            考试形式：{schedule.Format}
            试卷编号：{schedule.ExamPaperNumber}
            """,
        _ => throw new NotImplementedException(),
    };

    #endregion

    #region Alarm

    /// <summary>
    /// 通过传入的类来设置提醒，如果返回 null，则使用默认提醒（即 <see cref="Alarm"/>）
    /// </summary>
    public event Func<T, Alarm?>? SetAlarmFunc;

    /// <summary>默认提醒</summary>
    public Alarm? Alarm = null;

    /// <summary>是否逆序遍历</summary>
    public bool IsReverseOrder = false;

    /// <remarks>仅在转换时调用</remarks>
    public Alarm? GetAlarm(T source)
    {
        var result = Alarm;
        if (SetAlarmFunc is not null)
        {
            foreach (var item in SetAlarmFunc.GetInvocationList())
            {
                result = (item as Func<T, Alarm?>)?.Invoke(source) ?? result;
            }
        }
        return result;
    }

    #endregion
}
