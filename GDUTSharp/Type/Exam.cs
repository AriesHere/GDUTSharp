using System.Text.Json.Serialization;
using GDUTSharp.Json;

namespace GDUTSharp.Type;

public class ExamCollection
{
    [JsonPropertyName("rows")]
    public List<Exam> Items { get; set; } = [];

    public int Count => Items.Count;

    public void Add(Exam item) => Items.Add(item);

    public void Clear() => Items.Clear();

    public void CopyTo(Exam[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

    public Exam this[int index] => Items[index];
}

/// <remarks>
/// <para>务必注意: <see cref="Time"/> 才是真正的考试时间 <see cref="Sessions"/> 只是占用节次</para>
/// <para>TODO: 部分属性仍可继续拆分</para>
/// </remarks>
public class Exam
{
    [JsonPropertyName("jkteaxms")]
    public string Teachers { get; set; } = string.Empty;

    [JsonPropertyName("ksrq")]
    public DateOnly Date { get; set; } = DateOnly.MinValue;

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

    [JsonPropertyName("kssj")]
    public string Time { get; set; } = string.Empty;

    /// <summary>考试类别</summary>
    [JsonPropertyName("kslbmc")]
    public string ExamType { get; set; } = string.Empty;

    [JsonPropertyName("xqmc")]
    public string Campus { get; set; } = string.Empty;

    /// <summary>安排类型</summary>
    [JsonPropertyName("ksaplxmc")]
    public string ScheduleType { get; set; } = string.Empty;

    [JsonPropertyName("kcmc")]
    public string Name { get; set; } = string.Empty;

    /// <summary>试卷编号</summary>
    [JsonPropertyName("sjbh")]
    public string ExamPaperNumber { get; set; } = string.Empty;

    /// <summary>考试形式</summary>
    /// <remarks> TODO: 我只见过闭卷考试，这个值是 0，开卷不清楚 </remarks>
    [JsonPropertyName("ksxs")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("kscdmc")]
    public string Location { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"""
        Exam:
          - Teachers:{Teachers}
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
