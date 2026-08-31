using GDUTSharp.Extra;
using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared.Type;
using Ical.Net;     // 这两个依赖仅在导出课程功能需要
using Ical.Net.DataTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Example;

public class Program
{
    public static IHost? AppHost = null;

    public static async Task Main(string[] args)
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
            var loginResult = await dataService.Login(testLoginInfo);
            var authResult = await dataService.Auth(IDataService.SupportedServices.JXFW);
            logger.LogInformation("登录结果:{Result}, 认证结果:{Result}", loginResult, authResult);

            // 获取学期
            var term = dataService.GetTerm().Result;
            if (term is not null
                && await dataService.GetLessons(term) is List<Lesson> lessons)
            {
                // 以下是 GDUTSharp.Extra 的功能之一：导出课程为 iCalendar 文件以便于导入到 outlook 日历或 Google 日历
                await File.WriteAllTextAsync("path/to/file",
                    lessons.ToCalendarString(new Extensions.ICalConvertContext()
                        {
                            Alarm = new()
                            {
                                Trigger = new(new Duration(minutes: -30)),
                                Description = "课程",
                                Action = AlarmAction.Display
                            },
                            IsMergeIfContinuous = true,
                        }
                    ));
            }
        }
    }
}
