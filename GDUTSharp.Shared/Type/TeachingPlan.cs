namespace GDUTSharp.Shared.Type;

public class AvaliableTeachingPlan
{
    /// <summary>教学计划代码</summary>
    public string Code { get; set; } = string.Empty;

    public string Year { get; set; } = string.Empty;

    /// <summary>计划类型，如“公选课计划”、“培养方案”</summary>
    public string PlanType { get; set; } = string.Empty;

    /// <summary>适用对象，如“20XX春全校公选课计划”</summary>
    public string Target { get; set; } = string.Empty;
}

public class TeachingPlan
{
    /// <summary>模块方向</summary>
    public string ProgramName { get; set; } = string.Empty;

    /// <summary>课程编号</summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>课程名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>课程英文名称</summary>
    public string Name_en { get; set; } = string.Empty;

    /// <summary>课程平台</summary>
    /// <remarks>
    /// 没看太懂，应该是指负责的部门<br/>
    /// 如：<br/>
    /// 外语[理:36,实:12]<br/>
    /// 马克思[理:6,实:10]<br/>
    /// 其中方括号内的“理”应该表示“理论教学”，“实”则应该表示“实验教学”<br/>
    /// 数字应该是学时<br/>
    /// </remarks>
    public string Department { get; set; } = string.Empty;

    /// <summary>课程大类</summary>
    /// <remarks>即"专业基础课"等</remarks>
    public string Type { get; set; } = string.Empty;

    /// <summary>课程分类</summary>
    /// <remarks>即"自然科学与工程技术类"等</remarks>
    public string Category { get; set; } = string.Empty;

    /// <summary>学分</summary>
    public float Credit { get; set; }

    /// <summary>学时</summary>
    public int ClassHour { get; set; }

    /// <summary>成绩方式，如“百分制”</summary>
    public string GradeScale { get; set; } = string.Empty;

    /// <summary>修读方式</summary>
    public string StudyMode { get; set; } = string.Empty;

    /// <summary>学年学期代码，如“202601”</summary>
    public string Term { get; set; } = string.Empty;
}
