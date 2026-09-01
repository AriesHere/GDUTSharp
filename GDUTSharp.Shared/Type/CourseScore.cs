namespace GDUTSharp.Shared.Type;

public partial class CourseScore
{
    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string Category { get; set; } = string.Empty;

    /// <summary>学年学期</summary>
    public string Term { get; set; } = string.Empty;

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string Type { get; set; } = string.Empty;

    /// <summary>绩点</summary>
    public float Gp { get; set; }

    /// <summary>课程名称</summary>
    public string Name { get; set; } = string.Empty;

    public int Score {  get; set; }

    /// <summary>考试性质</summary>
    public string ExamType { get; set; } = string.Empty;

    /// <summary>学分</summary>
    public float Credit { get; set; }

    /// <summary>学时</summary>
    public int ClassHour { get; set; }

    /// <summary>修读方式</summary>
    public string StudyMode { get; set; } = string.Empty;

    /// <summary>成绩方式</summary>
    /// <remarks>TODO: 只见过百分制，不知道是否存在其它方式。如果存在，注意修改 <see cref="Score"/> 的类型</remarks>
    public string GradeScale { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        LessonScore:
          - Category:{Category}
          - Term:{Term}
          - Type:{Type}
          - Gp:{Gp}
          - Name:{Name}
          - Score:{Score}
          - ExamType:{ExamType}
          - Credit:{Credit}
          - ClassHour:{ClassHour}
          - StudyMode:{StudyMode}
          - GradeScale:{GradeScale}
        """;
    }
}
