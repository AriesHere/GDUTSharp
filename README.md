# GDUTSharp

受 [gdutday/gdutday-wechat3.0-java](https://github.com/gdutday/gdutday-wechat3.0-java) 启发而开发的一个能便捷地从广东工业大学各系统中获取数据的 C# 库，以便于后续开发  
目前仅支持本科生相关的部分  
支持 AOT  

## 功能

GDUTSharp:
- 统一认证中心
    - 登录与注销
- 教学服务系统
    - 课表信息
    - 考试安排信息
    - 课程成绩信息
    - 选课页面 => 可选课程和已选课程信息
- 图书馆
    - 每日推荐
    - 借阅信息
- 其它
    - 绩点计算

GDUTSharp.Extra:
- 为部分功能提供更稳健的实现
- 读取从教学服务中心导出的数据
- 将课表信息和考试安排信息输出为 [iCalendar](https://icalendar.org/) 文件

[TODO](TODO.md)

## 示例  
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
