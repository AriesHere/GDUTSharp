using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Example
{
    public class Program
    {
        public static IHost? AppHost = null;

        public static void Main(string[] args)
        {
            // 注册到 DI 容器中
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // 使用此方法以自动完成对CommonClient的所有配置
                    services.AddCommonClient(context.Configuration);

                    services.AddScoped<ICookieService, CookieService>();
                    services.AddScoped<IDataService, DataService>();
                    services.AddSingleton<ISecurityService, SecurityService>();
                })
                .Build();

            AppHost.RunAsync();

            // 这里填充学号和密码
            LoginInfo testLoginInfo = new() { UserName = "", Password = "" };

            var sc = AppHost.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = sc.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

                // 先登录，再认证
                // 如果学号密码正确，这两个结果都是 true
                // 自行补充学号密码错误时的处理逻辑
                var loginResult = dataService.Login(testLoginInfo).Result;
                logger.LogInformation("登录结果:{Result}", loginResult);

                var authResult = dataService.Auth(IDataService.SupportedServices.JXFW).Result;
                logger.LogInformation("认证结果:{Result}", authResult);

                // 这里可以先通过 GetTerm 方法获取学期代码
                var result = dataService.GetExamSchedule("202502").Result;
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        // 输出
                        logger.LogInformation("{Result}", item);
                    }
                }
            }
        }
    }
}
