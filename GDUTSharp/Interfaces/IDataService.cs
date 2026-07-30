using GDUTSharp.Type;

namespace GDUTSharp.Interfaces
{
    public interface IDataService
    {
        /// <summary>
        /// 统一认证中心登录，必须先后执行 <see cref="Login(LoginInfo)"/> 和 <see cref="Auth(string)"/> 之后才能进行其它操作。
        /// </summary>
        /// <remarks>
        /// 返回 ture 即表示登录成功。
        /// 对于同一用户，无需反复登录。对于同一用户在同一系统的操作，无需反复认证。
        /// </remarks>
        public bool Login(LoginInfo user);

        /// <summary>
        /// 通过统一认证中心认证
        /// </summary>
        public bool Auth(string url);

        /// <summary>
        /// 获取学期信息
        /// </summary>
        public string? GetTerm();

        /// <summary>
        /// 获取课表信息
        /// </summary>
        public LessonCollection? GetLessons(string term);

        /// <summary>
        /// 获取考试安排信息
        /// </summary>
        public ExamCollection? GetExamSchedule(string term);

        /// <summary>
        /// 获取考试成绩信息
        /// </summary>
        public LessonScoreCollection? GetScore(string term);

        /// <summary>
        /// 获取选课页面中显示的可选课程
        /// </summary>
        public CourseSelCollection? GetCourseSelection();

        /// <summary>
        /// 获取选课页面中显示的已选课程
        /// </summary>
        public List<CourseSelection>? GetSelectedCourse();
    }
}
