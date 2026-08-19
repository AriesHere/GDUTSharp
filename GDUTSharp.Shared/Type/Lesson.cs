namespace GDUTSharp.Shared.Type;

public class Lesson
{
    /// <summary>课程名称</summary>
    public string Name { get; set; } = string.Empty;

    public List<string> ClassName { get; set; } = [];

    public int StudentsCount { get; set; } = 0;

    public string Teacher { get; set; } = string.Empty;

    public int Week { get; set; } = 0;

    public int DayOfWeek { get; set; } = 0;

    public List<int> Sessions { get; set; } = [];

    public string Location { get; set; } = string.Empty;

    public DateOnly Date { get; set; } = DateOnly.MinValue;

    /// <summary>课序</summary>
    public int LessonSequence { get; set; } = 0;

    /// <summary>教学环节，即"理论教学"等</summary>
    public string LessonType { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;

    public string Term { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        Lesson:
          - Name:{Name}
          - ClassName:{string.Join(",", ClassName)}
          - StudentsCount:{StudentsCount}
          - Teacher:{Teacher}
          - Week:{Week}
          - DayOfWeek:{DayOfWeek}
          - Sessions:{string.Join(",", Sessions)}
          - Location:{Location}
          - Date:{Date}
          - LessonSequence:{LessonSequence}
          - LessonType:{LessonType}
          - Profile:{Profile}
        """;
    }
}
