using GDUTSharp.Shared.Type;

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
        public Task<bool> Login(LoginInfo user);

        /// <remarks>
        /// 注意：即使未登录，调用本方法也会返回 true，因为统一认证中心的登出操作是幂等的
        /// </remarks>
        public Task<bool> Logout();

        /// <summary>
        /// 通过统一认证中心认证
        /// </summary>
        public Task<bool> Auth(string url);

        public Task<bool> Auth(SupportedServices service);

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
        /// 获取考试成绩信息
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

        /// <summary>
        /// 获取课程任务
        /// </summary>
        /// <param name="CourseTaskCode">课程任务代码</param>
        public Task<List<Lesson>?> GetCourseTask(string CourseTaskCode);

        public Task<List<BorrowedBook>?> GetBorrowedBooks();

        public enum SupportedServices
        {
            JXFW,       // 教学服务系统
            LIBRARY,    // 图书馆
        }
    }
}
