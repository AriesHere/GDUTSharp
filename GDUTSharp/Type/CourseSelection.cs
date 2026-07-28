using System.Text.Json.Serialization;

namespace GDUTSharp.Type;

public class CourseSelCollection
{
    [JsonPropertyName("rows")]
    public List<CourseSelection> Items { get; set; } = [];

    public int Count => Items.Count;

    public void Add(CourseSelection item) => Items.Add(item);

    public void Clear() => Items.Clear();

    public void CopyTo(CourseSelection[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public CourseSelection this[int index] => Items[index];
}

public class CourseSelection
{
    /// <summary>课程任务代码</summary>
    [JsonPropertyName("kcrwdm")]
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>排课人数</summary>
    [JsonPropertyName("pkrs")]
    public string StudentsCount { get; set; } = string.Empty;

    /// <summary>课程简介</summary>
    [JsonPropertyName("kcptdm")]
    public string Profile { get; set; } = string.Empty;

    /// <summary>培养项目名称</summary>
    [JsonPropertyName("xmmc")]
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    [JsonPropertyName("kcmc")]
    public string Name { get; set; } = string.Empty;

    /// <summary>学时</summary>
    [JsonPropertyName("zxs")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ClassHour { get; set; }

    /// <summary>学分</summary>
    [JsonPropertyName("xf")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float Credit { get; set; }

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    [JsonPropertyName("kcdlmc")]
    public string Type { get; set; } = string.Empty;

    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    [JsonPropertyName("kcflmc")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("teaxm")]
    public string Teacher { get; set; } = string.Empty;

    /// <summary>已选人数</summary>
    [JsonPropertyName("jxbrs")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
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
