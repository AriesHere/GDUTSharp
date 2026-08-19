namespace GDUTSharp.Shared.Type;

public class CourseSel
{
    /// <summary>课程任务代码</summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>排课人数</summary>
    public string StudentsCount { get; set; } = string.Empty;

    /// <summary>课程简介</summary>
    public string Profile { get; set; } = string.Empty;

    /// <summary>培养项目名称</summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>学时</summary>
    public int ClassHour { get; set; }

    /// <summary>学分</summary>
    public float Credit { get; set; }

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string Type { get; set; } = string.Empty;

    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string Category { get; set; } = string.Empty;

    public string Teacher { get; set; } = string.Empty;

    /// <summary>已选人数</summary>
    public int EnrolledCount { get; set; }

    public override string ToString()
    {
        return $"""
        CourseSelection:
          - CourseCode:{CourseCode}
          - StudentsCount:{StudentsCount}
          - Profile:{Profile}
          - ProgramName:{ProgramName}
          - Name:{Name}
          - ClassHour:{ClassHour}
          - Credit:{Credit}
          - Type:{Type}
          - Category:{Category}
          - Teacher:{Teacher}
          - EnrolledCount:{EnrolledCount}
        """;
    }
}
