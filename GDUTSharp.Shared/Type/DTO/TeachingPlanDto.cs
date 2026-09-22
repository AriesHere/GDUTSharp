#pragma warning disable IDE1006 // Naming Styles

using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Type.DTO;

public class AvaliableTeachingPlanDtoCollection : DtoCollectionBase<AvaliableTeachingPlanDto>
{
    public static implicit operator List<AvaliableTeachingPlan>(AvaliableTeachingPlanDtoCollection? collection) => collection is null ? [] : [.. collection];
}

public class AvaliableTeachingPlanDto
{
    /// <summary>教学计划代码</summary>
    public string jxjhdm { get; set; } = string.Empty;

    /// <summary>年级，如“2026”</summary>
    public string nd { get; set; } = string.Empty;

    /// <summary>计划类型，如“公选课计划”、“培养方案”</summary>
    public string jhlxmc { get; set; } = string.Empty;

    /// <summary>适用对象，如“20XX春全校公选课计划”</summary>
    public string sydx { get; set; } = string.Empty;

    public static implicit operator AvaliableTeachingPlan(AvaliableTeachingPlanDto dto)
    {
        return new AvaliableTeachingPlan
        {
            Code = dto.jxjhdm,
            Year = dto.nd,
            PlanType = dto.jhlxmc,
            Target = dto.sydx,
        };
    }
}

public class TeachingPlanDtoCollection : DtoCollectionBase<TeachingPlanDto>
{
    public static implicit operator List<TeachingPlan>(TeachingPlanDtoCollection? collection) => collection is null ? [] : [.. collection];
}

public class TeachingPlanDto
{
    /// <summary>模块方向</summary>
    public string jhfxmc { get; set; } = string.Empty;

    /// <summary>课程编号</summary>
    public string kcbh { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string kcmc { get; set; } = string.Empty;

    /// <summary>课程英文名称</summary>
    public string kcywmc { get; set; } = string.Empty;

    /// <summary>课程平台</summary>
    /// <remarks>
    /// 没看太懂，应该是指负责的部门<br/>
    /// 如：<br/>
    /// 外语[理:36,实:12]<br/>
    /// 马克思[理:6,实:10]<br/>
    /// 其中方括号内的“理”应该表示“理论教学”，“实”则应该表示“实验教学”<br/>
    /// 数字应该是学时<br/>
    /// </remarks>
    public string kcptmc { get; set; } = string.Empty;

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string kcdlmc { get; set; } = string.Empty;

    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string kcflmc { get; set; } = string.Empty;

    /// <summary>学分</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float xf { get; set; }

    /// <summary>学时</summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int zxs { get; set; }

    /// <summary>成绩方式，如“百分制”</summary>
    public string cjfsmc { get; set; } = string.Empty;

    /// <summary>修读方式</summary>
    public string xdfsmc { get; set; } = string.Empty;

    /// <summary>学年学期代码，如“202601”</summary>
    public string xnxqdm1 { get; set; } = string.Empty;

    public static implicit operator TeachingPlan(TeachingPlanDto dto)
    {
        return new TeachingPlan
        {
            ProgramName = dto.jhfxmc,
            CourseCode = dto.kcbh,
            Name = dto.kcmc,
            Name_en = dto.kcywmc,
            Department = dto.kcptmc,
            Type = dto.kcdlmc,
            Category = dto.kcflmc,
            Credit = dto.xf,
            ClassHour = dto.zxs,
            GradeScale = dto.cjfsmc,
            StudyMode = dto.xdfsmc,
            Term = dto.xnxqdm1,
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
