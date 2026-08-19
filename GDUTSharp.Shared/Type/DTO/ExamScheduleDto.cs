using System.Text.Json.Serialization;
using GDUTSharp.Shared.Json;

namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class ExamScheduleDtoCollection : DtoCollectionBase<ExamScheduleDto>
{
    public static implicit operator List<ExamSchedule>(ExamScheduleDtoCollection collection) => [.. collection];
}

/// <remarks>
/// <para>务必注意: <see cref="Time"/> 才是真正的考试时间 <see cref="Sessions"/> 只是占用节次</para>
/// <para>TODO: 部分属性仍可继续拆分</para>
/// </remarks>
public class ExamScheduleDto
{
    /// <summary>监考老师</summary>
    [JsonConverter(typeof(SplittedStringConverter))]
    public List<string> jkteaxms { get; set; } = [];

    /// <summary>考试日期</summary>
    public DateOnly ksrq { get; set; } = DateOnly.MinValue;

    /// <summary>周次</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zc { get; set; } = 0;

    /// <summary>星期</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int xq { get; set; } = 0;

    #region Sessions

    /// <summary>占用节次</summary>
    [JsonIgnore]
    public List<int> Sessions { get; set; } = [];

    [JsonConverter(typeof(SessionsConverter))]
    public List<int> jcdm { set => Sessions = value ?? []; }

    [JsonConverter(typeof(SessionsConverter2))]
    public List<int> jcdm2 { set => Sessions = value ?? []; }

    #endregion

    /// <summary>考试时间</summary>
    public string kssj { get; set; } = string.Empty;

    /// <summary>考试类别</summary>
    public string kslbmc { get; set; } = string.Empty;

    /// <summary>校区</summary>
    public string xqmc { get; set; } = string.Empty;

    /// <summary>安排类型</summary>
    public string ksaplxmc { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string kcmc { get; set; } = string.Empty;

    /// <summary>试卷编号</summary>
    public string sjbh { get; set; } = string.Empty;

    /// <summary>考试形式</summary>
    /// <remarks> TODO: 我只见过闭卷考试，这个值是 0，开卷不清楚 </remarks>
    public string ksxs { get; set; } = string.Empty;

    /// <summary>考场</summary>
    public string kscdmc { get; set; } = string.Empty;

    public static implicit operator ExamSchedule(ExamScheduleDto dto)
    {
        return new()
        {
            Teachers = dto.jkteaxms,
            Date = dto.ksrq,
            Week = dto.zc,
            DayOfWeek = dto.xq,
            Sessions = dto.Sessions,
            Time = dto.kssj,
            ExamType = dto.kslbmc,
            Campus = dto.xqmc,
            ScheduleType = dto.ksaplxmc,
            Name = dto.kcmc,
            ExamPaperNumber = dto.sjbh,
            Format = dto.ksxs,
            Location = dto.kscdmc,
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
