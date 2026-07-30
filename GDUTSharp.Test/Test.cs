using System.Diagnostics;
using GDUTSharp.Interfaces;
using GDUTSharp.Type;
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
        private IServiceScope _scope;
        private IDataService _service;
        private LoginInfo _testInfo;
        private string _testTerm;
        private string _courseTaskCode;
        private int _flag = 0;

        [TestInitialize]
        public void TestInit()
        {
            var dbopt = General.Host.Services.GetRequiredService<IOptions<DebugOptions>>();
            _testInfo = new() { UserName = dbopt.Value.UserName, Password = dbopt.Value.Password };
            _testTerm = dbopt.Value.Term;
            _scope = General.Host.Services.CreateScope();
            _service = _scope.ServiceProvider.GetRequiredService<IDataService>();
        }

        [TestCleanup]
        public void TestCleanup() => this.Dispose();
        public void Dispose() => _scope.Dispose();

        [TestMethod]
        [Priority(0)]
        public void TestJXFW()
        {
            bool r = _service.Login(_testInfo);
            if (!r)
            {
                Assert.Fail("登录失败");
                return;
            }

            r = _service.Auth(Constant.UNDER_GRADUATE_LOGIN);
            if (!r)
            {
                Assert.Fail("认证失败");
                return;
            }

            dynamic? result;

            result = _service.GetTerm();
            Assert.IsNotNull(result, "获取学期代码失败");

            result = _service.GetScore(_testTerm);
            Assert.IsNotNull(result, "获取课程成绩失败");

            result = _service.GetLessons(_testTerm);
            Assert.IsNotNull(result, "获取课程安排失败");

            result = _service.GetExamSchedule(_testTerm);
            Assert.IsNotNull(result, "获取考试安排失败");

            result = _service.GetCourseSelection();
            Assert.IsNotNull(result, "获取选课列表失败");

            result = _service.GetSelectedCourse();
            Assert.IsNotNull(result, "获取已选课列表失败");

            result = _service.GetCourseTask(_courseTaskCode);
            Assert.IsNotNull(result, "获取课程任务失败");
        }
    }
}
