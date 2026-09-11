# GDUTSharp

对 [gdutday/gdutday-wechat3.0-java](https://github.com/gdutday/gdutday-wechat3.0-java) 的 C# 不完全重实现，现在加入了一些独占功能  
目前仅支持本科生相关的部分  
支持 AOT  
示例:  
```C#
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
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<ILibraryService, LibraryService>();
                services.AddScoped<IJXFWService, JXFWService>();
                services.AddSingleton<ISecurityService, SecurityService>();
                // 使用此方法以自动完成对CommonClient的所有配置
                services.AddCommonClient(context.Configuration);
            })
            .Build();
        AppHost.RunAsync();

        // 这里填充学号和密码
        LoginInfo testLoginInfo = new() { UserName = "", Password = "" };

        var sc = AppHost.Services.GetRequiredService<IServiceScopeFactory>();
        using (var scope = sc.CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var jxfw = scope.ServiceProvider.GetRequiredService<IJXFWService>();

            // 自行补充学号密码错误时的处理逻辑
            var loginResult = await jxfw.Login(testLoginInfo);
            logger.LogInformation("登录结果:{Result}", loginResult);

            // 获取学期
            var term = jxfw.GetTerm().Result;
            if (term is not null
                && await jxfw.GetLessons(term) is List<Lesson> lessons)
            {
                // 以下是 GDUTSharp.Extra 的功能之一：导出课程为 iCalendar 文件以便于导入到 outlook 日历或 Google 日历
                await File.WriteAllTextAsync("path/to/file",
                    lessons.ToCalendarString(new ExtraExtensions.ICalConvertContext()
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
```
