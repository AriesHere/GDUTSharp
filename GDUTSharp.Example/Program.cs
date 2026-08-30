using GDUTSharp.Extra;
using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared.Type;
using Ical.Net.CalendarComponents;  // 这两个依赖，仅在导出课程功能需要
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
            logger.LogInformation("登录结果:{Result}", loginResult);

            var authResult = await dataService.Auth(IDataService.SupportedServices.JXFW);
            logger.LogInformation("认证结果:{Result}", authResult);

            // 获取学期
            var term = dataService.GetTerm().Result;
            if (term is not null)
            {
                var lessons = await dataService.GetLessons(term);
                if (lessons is not null)
                {
                    // 以下是 GDUTSharp.Extra 的功能之一：导出课程为 iCalendar 文件以便于导入到 outlook 日历或 Google 日历
                    Alarm alarm = new()
                    {
                        Trigger = new(new Duration(minutes: 15))
                    };
                    await File.WriteAllTextAsync("path/to/file", lessons.ToCalendarString());
                }
            }
        }
    }
}
