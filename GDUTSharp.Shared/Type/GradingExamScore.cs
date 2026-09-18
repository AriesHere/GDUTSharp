namespace GDUTSharp.Shared.Type;

[Attributes.OverrideToString]
public partial class GradingExamScore
{
    /// <summary>学年学期名称，如“2026秋季”</summary>
    public string Term { get; set; } = string.Empty;

    /// <summary>考级课程编号，如“CET6”</summary>
    public string GradingExamCode { get; set; } = string.Empty;

    /// <summary>考级课程名称，如”英语六级“</summary>
    public string GradingExamName { get; set; } = string.Empty;

    /// <summary>考级时间</summary>
    public DateOnly Date { get; set; } = DateOnly.MinValue;

    /// <summary>准考证号</summary>
    public string PermissionNumber { get; set; } = string.Empty;

    /// <summary>总成绩</summary>
    public string Score { get; set; } = string.Empty;

    /// <summary>细项成绩</summary>
    /// <remarks>
    /// 索引:
    /// <para>
    /// 英语四六级：<br/>
    ///     [0] => 听力成绩<br/>
    ///     [1] => 阅读成绩<br/>
    ///     [2] => 写作成绩<br/>
    ///     [3] => 口语成绩<br/>
    /// </para>
    /// </remarks>
    public List<string> ScoreDetail { get; set; } = [];
}
