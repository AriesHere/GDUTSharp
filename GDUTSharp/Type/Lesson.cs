using System.Text.Json.Serialization;
using GDUTSharp.Json;

namespace GDUTSharp.Type;

/// <remarks>
/// 在 <see cref="Services.DataService.GetLessons"/> 方法中通过 { "sort", "zc,xq,jcdm" } 来保证按时间排序，以获得更好的性能
/// </remarks>
public class LessonCollection
{
    [JsonPropertyName("rows")]
    public List<Lesson> Items { get; set; } = [];

    public int Count => Items.Count;

    public void Add(Lesson item) => Items.Add(item);

    public void Clear() => Items.Clear();

    public void CopyTo(Lesson[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public Lesson this[int index] => Items[index];
}

public class Lesson
{
    /// <summary>课程名称</summary>
    [JsonPropertyName("kcmc")]
    public string Name { get; set; } = string.Empty;

    #region ClassName

    /// <summary>教学班名称</summary>
    [JsonIgnore]
    public List<string> ClassName { get; set; } = [];

    [JsonPropertyName("jxbmc")]
    [JsonConverter(typeof(ClassNameConverter))]
    public List<string> ClassNameSetter
    {
        set => ClassName = value ?? [];
    }

    [JsonPropertyName("ClassName")]
    public List<string> ClassNameWriter => ClassName;

    #endregion

    [JsonPropertyName("pkrs")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int StudentsCount { get; set; } = 0;

    [JsonPropertyName("teaxms")]
    public string Teacher { get; set; } = string.Empty;

    [JsonPropertyName("zc")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Week { get; set; } = 0;

    [JsonPropertyName("xq")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int DayOfWeek { get; set; } = 0;

    #region Sessions

    [JsonIgnore]
    public List<int> Sessions { get; set; } = [];

    [JsonPropertyName("jcdm")]
    [JsonConverter(typeof(SessionsConverter))]
    public List<int> JcdmSetter
    {
        set => Sessions = value ?? [];
    }

    [JsonPropertyName("jcdm2")]
    [JsonConverter(typeof(SessionsConverter2))]
    public List<int> Jcdm2Setter
    {
        set => Sessions = value ?? [];
    }

    [JsonPropertyName("Sessions")]
    public List<int> SessionsWriter => Sessions;

    #endregion

    [JsonPropertyName("jxcdmc")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("pkrq")]
    public DateOnly Date { get; set; } = DateOnly.MinValue;

    [JsonPropertyName("kxh")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int LessonSequence { get; set; } = 0;

    [JsonPropertyName("jxhjmc")]
    public string LessonType { get; set; } = string.Empty;

    [JsonPropertyName("sknrjj")]
    public string Profile { get; set; } = string.Empty;

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
