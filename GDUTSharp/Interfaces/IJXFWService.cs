using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 教学服务系统
/// </summary>
public interface IJXFWService
{
    public Task<bool> Login(LoginInfo? loginInfo = null);

    /// <summary>学期信息</summary>
    public Task<Term?> GetTerm();

    /// <summary>课表信息</summary>
    /// <remarks><paramref name="week"/> 为 null 时获取的是对应学期的所有课程</remarks>
    public Task<List<Lesson>?> GetLessons(Term term, int? week = null);

    /// <summary>考试安排信息</summary>
    public Task<List<ExamSchedule>?> GetExamSchedule(Term term);

    /// <summary>课程成绩信息</summary>
    public Task<List<CourseScore>?> GetCourseScore(Term? term = null);

    /// <summary>选课页面中显示的可选课程</summary>
    public Task<List<CourseSel>?> GetCourseSelection();

    /// <summary>选课页面中显示的已选课程</summary>
    public Task<List<CourseSel>?> GetSelectedCourse();

    /// <summary>课程任务</summary>
    /// <remarks><paramref name="code"/> 是课程任务代码</remarks>
    /// <param name="code">课程任务代码</param>
    public Task<List<Lesson>?> GetCourseTask(string code);

    /// <summary>考级成绩</summary>
    public Task<List<GradingExamScore>?> GetGradingExamScore();

    /// <summary>获取教学计划列表</summary>
    public Task<List<AvaliableTeachingPlan>?> GetTeachingPlanList();

    /// <summary>获取教学计划详情</summary>
    public Task<List<TeachingPlan>?> GetTeachingPlan(string teachingPlanCode);

    /// <summary>获取学期注册信息</summary>
    public Task<List<SemesterReg>?> GetSemesterRegistration();
}
