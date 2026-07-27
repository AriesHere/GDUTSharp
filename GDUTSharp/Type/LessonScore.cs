using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GDUTSharp.Type;

public class LessonScoreCollection
{
    [JsonPropertyName("rows")]
    public List<LessonScore> Lessons { get; set; } = [];

    public int Count => Lessons.Count;

    public void Add(LessonScore item) => Lessons.Add(item);

    public void Clear() => Lessons.Clear();

    public void CopyTo(LessonScore[] array, int arrayIndex) => Lessons.CopyTo(array, arrayIndex);

    public LessonScore this[int index] => Lessons[index];
}

public class LessonScore
{
    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    [JsonPropertyName("kcflmc")]
    public string Category { get; set; } = string.Empty;

    /// <summary>学年学期</summary>
    [JsonPropertyName("xnxqmc")]
    public string Term { get; set; } = string.Empty;

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    [JsonPropertyName("kcdlmc")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("cjjd")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float Gpa { get; set; }

    /// <summary>课程名称</summary>
    [JsonPropertyName("kcmc")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("zcj")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Score {  get; set; }

    /// <summary>考试性质</summary>
    [JsonPropertyName("ksxzmc")]
    public string ExamType { get; set; } = string.Empty;

    /// <summary>学分</summary>
    [JsonPropertyName("xf")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float Credit { get; set; }

    /// <summary>学时</summary>
    [JsonPropertyName("zxs")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ClassHour { get; set; }

    /// <summary>修读方式</summary>
    [JsonPropertyName("xdfsmc")]
    public string StudyMode { get; set; } = string.Empty;

    /// <summary>成绩方式</summary>
    /// <remarks>TODO: 只见过百分制，不知道是否存在其它方式。如果存在，注意修改 <see cref="Score"/> 的类型</remarks>
    [JsonPropertyName("cjfsmc")]
    public string GradeScale { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        LessonScore:
          - Category:{Category}
          - Term:{Term}
          - Type:{Type}
          - Gpa:{Gpa}
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
