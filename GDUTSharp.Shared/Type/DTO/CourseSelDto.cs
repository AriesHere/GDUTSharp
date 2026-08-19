using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class CourseSelDtoCollection : DtoCollectionBase<CourseSelDto>
{
    public static implicit operator List<CourseSel>(CourseSelDtoCollection collection) => [..collection];
}

public class CourseSelDto
{
    /// <summary>课程任务代码</summary>
    public string kcrwdm { get; set; } = string.Empty;

    /// <summary>排课人数</summary>
    public string pkrs { get; set; } = string.Empty;

    /// <summary>课程简介</summary>
    public string kcptdm { get; set; } = string.Empty;

    /// <summary>培养项目名称</summary>
    public string xmmc { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string kcmc { get; set; } = string.Empty;

    /// <summary>学时</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zxs { get; set; }

    /// <summary>学分</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float xf { get; set; }

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string kcdlmc { get; set; } = string.Empty;

    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string kcflmc { get; set; } = string.Empty;

    public string teaxm { get; set; } = string.Empty;

    /// <summary>已选人数</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int jxbrs { get; set; }

    public static implicit operator CourseSel(CourseSelDto dto)
    {
        return new()
        {
            CourseCode = dto.kcrwdm,
            StudentsCount = dto.pkrs,
            Profile = dto.kcptdm,
            ProgramName = dto.xmmc,
            Name = dto.kcmc,
            ClassHour = dto.zxs,
            Credit = dto.xf,
            Type = dto.kcdlmc,
            Category = dto.kcflmc,
            Teacher = dto.teaxm,
            EnrolledCount = dto.jxbrs
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
