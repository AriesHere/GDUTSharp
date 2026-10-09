using System.Net;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 教学服务系统
/// </summary>
public interface IJXFWService
{
    /// <returns>JFIF 格式</returns>
    public Task<byte[]?> GetCaptcha(CookieContainer cookies, CancellationToken token = default);

    public Task<bool> Login(
        CookieContainer cookies,
        LoginInfo? loginInfo = null,
        LoginType loginType = LoginType.AuthServer,
        CancellationToken token = default);

    /// <summary>学期信息</summary>
    public Task<Term?> GetTerm(CookieContainer cookies, CancellationToken token = default);

    /// <summary>课表信息</summary>
    /// <remarks><paramref name="week"/> 为 null 时获取的是对应学期的所有课程</remarks>
    public Task<List<Lesson>?> GetLessons(CookieContainer cookies, Term term, int? week = null, CancellationToken token = default);

    /// <summary>考试安排信息</summary>
    public Task<List<ExamSchedule>?> GetExamSchedule(CookieContainer cookies, Term term, CancellationToken token = default);

    /// <summary>课程成绩信息</summary>
    public Task<List<CourseScore>?> GetCourseScore(CookieContainer cookies, Term? term = null, CancellationToken token = default);

    /// <summary>选课页面中显示的可选课程</summary>
    public Task<List<CourseSel>?> GetCourseSelection(CookieContainer cookies, CancellationToken token = default);

    /// <summary>选课页面中显示的已选课程</summary>
    public Task<List<CourseSel>?> GetSelectedCourse(CookieContainer cookies, CancellationToken token = default);

    /// <summary>课程任务</summary>
    /// <remarks><paramref name="code"/> 是课程任务代码</remarks>
    /// <param name="code">课程任务代码</param>
    public Task<List<Lesson>?> GetCourseTask(CookieContainer cookies, string code, CancellationToken token = default);

    /// <summary>考级成绩</summary>
    public Task<List<GradingExamScore>?> GetGradingExamScore(CookieContainer cookies, CancellationToken token = default);

    /// <summary>获取教学计划列表</summary>
    public Task<List<AvaliableTeachingPlan>?> GetTeachingPlanList(CookieContainer cookies, CancellationToken token = default);

    /// <summary>获取教学计划详情</summary>
    public Task<List<TeachingPlan>?> GetTeachingPlan(CookieContainer cookies, string teachingPlanCode, CancellationToken token = default);

    /// <summary>获取学期注册信息</summary>
    public Task<List<SemesterReg>?> GetSemesterRegistration(CookieContainer cookies, CancellationToken token = default);

    /// <summary>登录方式</summary>
    public enum LoginType
    {
        AuthServer, // 通过统一认证中心
        JXFW,       // 通过教学服务自有的登录方式
    }
}
