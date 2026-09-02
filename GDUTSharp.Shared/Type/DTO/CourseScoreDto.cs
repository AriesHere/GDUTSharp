using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class CourseScoreDtoCollection : DtoCollectionBase<CourseScoreDto>
{
    public static implicit operator List<CourseScore>(CourseScoreDtoCollection? collection) => collection is null ? [] : [.. collection];
}

public class CourseScoreDto
{
    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string kcflmc { get; set; } = string.Empty;

    /// <summary>学年学期</summary>
    public string xnxqmc { get; set; } = string.Empty;

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string kcdlmc { get; set; } = string.Empty;

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float cjjd { get; set; }

    /// <summary>课程名称</summary>
    public string kcmc { get; set; } = string.Empty;

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zcj { get; set; }

    /// <summary>考试性质</summary>
    public string ksxzmc { get; set; } = string.Empty;

    /// <summary>学分</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float xf { get; set; }

    /// <summary>学时</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zxs { get; set; }

    /// <summary>修读方式</summary>
    public string xdfsmc { get; set; } = string.Empty;

    /// <summary>成绩方式</summary>
    /// <remarks>TODO: 只见过百分制，不知道是否存在其它方式。如果存在，注意修改 <see cref="zcj"/> 的类型</remarks>
    public string cjfsmc { get; set; } = string.Empty;

    public static implicit operator CourseScore(CourseScoreDto dto)
    {
        return new()
        {
            Category = dto.kcflmc,
            Term = dto.xnxqmc,
            Type = dto.kcdlmc,
            Gp = dto.cjjd,
            Name = dto.kcmc,
            Score = dto.zcj,
            ExamType = dto.ksxzmc,
            Credit = dto.xf,
            ClassHour = dto.zxs,
            StudyMode = dto.xdfsmc,
            GradeScale = dto.cjfsmc,
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
