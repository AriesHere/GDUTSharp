using System.Text.Json.Serialization;
using GDUTSharp.Shared.Json;

namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

/// <remarks>
/// 在 <see cref="Services.DataService.GetLessons"/> 方法中通过 { "sort", "zc,xq,jcdm" } 来保证按时间排序，以获得更好的性能
/// </remarks>
public class LessonDtoCollection : DtoCollectionBase<LessonDto>
{
    public static implicit operator List<Lesson>(LessonDtoCollection? collection) => collection is null ? [] : [.. collection];
}

public class LessonDto
{
    /// <summary>课程名称</summary>
    public string kcmc { get; set; } = string.Empty;

    #region ClassName

    /// <summary>教学班名称</summary>
    [JsonIgnore]
    public List<string> ClassName { get; set; } = [];

    [JsonConverter(typeof(ClassNameConverter))]
    public List<string> jxbmc
    {
        set => ClassName = value ?? [];
    }

    #endregion

    /// <summary>排课人数</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int pkrs { get; set; } = 0;

    public string teaxms { get; set; } = string.Empty;

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zc { get; set; } = 0;

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int xq { get; set; } = 0;

    #region Sessions

    [JsonIgnore]
    public List<int> Sessions { get; set; } = [];

    [JsonConverter(typeof(SessionsConverter))]
    public List<int> jcdm { set => Sessions = value ?? []; }

    [JsonConverter(typeof(SessionsConverter2))]
    public List<int> jcdm2 { set => Sessions = value ?? []; }

    #endregion

    public string jxcdmc { get; set; } = string.Empty;

    public DateOnly pkrq { get; set; } = DateOnly.MinValue;

    /// <summary>课序</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int kxh { get; set; } = 0;

    /// <summary>教学环节，即"理论教学"等</summary>
    public string jxhjmc { get; set; } = string.Empty;

    public string sknrjj { get; set; } = string.Empty;

    /// <summary>学年学期代码</summary>
    public string xnxqdm { get; set; } = string.Empty;

    public static implicit operator Lesson(LessonDto dto)
    {
        return new()
        {
            Name = dto.kcmc,
            ClassName = dto.ClassName,
            StudentsCount = dto.pkrs,
            Teacher = dto.teaxms,
            Week = dto.zc,
            DayOfWeek = dto.xq,
            Sessions = dto.Sessions,
            Location = dto.jxcdmc,
            Date = dto.pkrq,
            LessonSequence = dto.kxh,
            LessonType = dto.jxhjmc,
            Profile = dto.sknrjj,
            Term = dto.xnxqdm
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
