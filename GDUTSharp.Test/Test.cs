using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Test
{
    public class DebugOptions
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Term { get; set; } = "202502";

        public string CourseTaskCode { get; set; } = string.Empty;
    }

    [TestClass]
    public sealed class Test : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private IServiceScope _scope;
        private IDataService _service;
        private LoginInfo _testInfo;
        private string _testTerm;
        private string _courseTaskCode;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [TestInitialize]
        public void TestInit()
        {
            var dbopt = General.Host.Services.GetRequiredService<IOptions<DebugOptions>>();
            _testInfo = new() { UserName = dbopt.Value.UserName, Password = dbopt.Value.Password };
            _testTerm = dbopt.Value.Term;
            _scope = General.Host.Services.CreateScope();
            _service = _scope.ServiceProvider.GetRequiredService<IDataService>();
            _courseTaskCode = dbopt.Value.CourseTaskCode;
        }

        [TestCleanup]
        public void TestCleanup() => this.Dispose();
        public void Dispose() => _scope.Dispose();

        [TestMethod]
        [Priority(0)]
        public void TestJXFW()
        {
            dynamic? result;

            result = _service.Login(_testInfo).Result;
            if (!result)
            {
                Assert.Fail("登录失败");
                return;
            }

            result = _service.Auth(Constant.UNDER_GRADUATE_LOGIN).Result;
            if (!result)
            {
                Assert.Fail("认证失败");
                return;
            }

            result = _service.GetTerm().Result;
            Assert.IsNotNull(result, "获取学期代码失败");

            result = _service.GetCourseScore(_testTerm).Result;
            Assert.IsNotNull(result, "获取课程成绩失败");

            result = _service.GetLessons(_testTerm).Result;
            Assert.IsNotNull(result, "获取课程安排失败");

            result = _service.GetExamSchedule(_testTerm).Result;
            Assert.IsNotNull(result, "获取考试安排失败");

            result = _service.GetCourseSelection().Result;
            Assert.IsNotNull(result, "获取选课列表失败");

            result = _service.GetSelectedCourse().Result;
            Assert.IsNotNull(result, "获取已选课列表失败");

            result = _service.GetCourseTask(_courseTaskCode).Result;
            Assert.IsNotNull(result, "获取课程任务失败");
        }
    }
}
