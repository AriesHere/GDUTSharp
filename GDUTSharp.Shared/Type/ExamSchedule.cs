namespace GDUTSharp.Shared.Type;

public partial class ExamSchedule
{
    /// <summary>监考老师</summary>
    public List<string> Teachers { get; set; } = [];

    /// <summary>考试日期</summary>
    public DateOnly Date { get; set; } = DateOnly.MinValue;

    public int Week { get; set; } = 0;

    public int DayOfWeek { get; set; } = 0;

    /// <summary>占用节次</summary>
    public List<int> Sessions { get; set; } = [];

    /// <summary>考试时间</summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>考试类别</summary>
    public string ExamType { get; set; } = string.Empty;

    /// <summary>校区</summary>
    public string Campus { get; set; } = string.Empty;

    /// <summary>安排类型</summary>
    public string ScheduleType { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>试卷编号</summary>
    public string ExamPaperNumber { get; set; } = string.Empty;

    /// <summary>考试形式</summary>
    /// <remarks> TODO: 我只见过闭卷考试，这个值是 0，开卷不清楚 </remarks>
    public string Format { get; set; } = string.Empty;

    /// <summary>考场</summary>
    public string Location { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        Exam:
          - Teachers:{string.Join(',', Teachers)}
          - Date:{Date}
          - Week:{Week}
          - DayOfWeek:{DayOfWeek}
          - Sessions:{string.Join(",", Sessions)}
          - Time:{Time}
          - ExamType:{ExamType}
          - Campus:{Campus}
          - ScheduleType:{ScheduleType}
          - Name:{Name}
          - ExamPaperNumber:{ExamPaperNumber}
          - Format:{Format}
          - Location:{Location}
        """;
    }
}
