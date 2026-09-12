using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>
/// 教学服务系统
/// </summary>
public interface IJXFWService
{
    public Task<bool> Login(LoginInfo? loginInfo = null);

    /// <summary>
    /// 获取学期信息
    /// </summary>
    public Task<string?> GetTerm();

    /// <summary>
    /// 获取课表信息
    /// </summary>
    public Task<List<Lesson>?> GetLessons(string term);

    /// <summary>
    /// 获取考试安排信息
    /// </summary>
    public Task<List<ExamSchedule>?> GetExamSchedule(string term);

    /// <summary>
    /// 获取课程成绩信息
    /// </summary>
    public Task<List<CourseScore>?> GetCourseScore(string term);

    /// <summary>
    /// 获取选课页面中显示的可选课程
    /// </summary>
    public Task<List<CourseSel>?> GetCourseSelection();

    /// <summary>
    /// 获取选课页面中显示的已选课程
    /// </summary>
    public Task<List<CourseSel>?> GetSelectedCourse();
}
