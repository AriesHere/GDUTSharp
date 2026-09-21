# GDUTSharp

受 [gdutday/gdutday-wechat3.0-java](https://github.com/gdutday/gdutday-wechat3.0-java) 启发而开发的一个从广东工业大学各系统中获取数据的 C# 库  
目前仅支持本科生相关的部分  

## 功能

开发计划: [TODO](TODO.md)

GDUTSharp (保证支持 AOT):
- 统一认证中心
    - 登录与注销
- 教学服务系统
    - 课表信息
    - 考试安排信息
    - 课程成绩信息
    - 选课页面 => 可选课程和已选课程信息
    - 考级成绩
- 图书馆
    - 每日推荐
    - 借阅信息
- 通知公文网
    - 通知数据
- 其它
    - 绩点计算

GDUTSharp.Extra:
- 为部分功能提供更稳健的实现
- 读取直接从教学服务中心导出的数据
- 将课表信息和考试安排信息输出为 [iCalendar](https://icalendar.org/) 文件
- 将课程数据，考试安排数据和选课数据导出为 xlsx 文件

## 使用示例  
```C#
using GDUTSharp.Extra;
using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// 这两个依赖仅在导出课程功能需要
using Ical.Net;
using Ical.Net.DataTypes;

// 注册到 DI 容器中
IHost AppHost = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // 如果需要隔离多用户，请 AddScoped
        services.AddSingleton<INoticeService, NoticeService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IJXFWService, JXFWService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        // 使用此方法以自动完成对CommonClient的所有配置
        services.AddCommonClient(context.Configuration);
    })
    .Build();
AppHost.RunAsync();

var logger = AppHost.Services.GetRequiredService<ILogger<Program>>();
var jxfw = AppHost.Services.GetRequiredService<IJXFWService>();

// 这里填充学号和密码
LoginInfo testLoginInfo = new() { UserName = "", Password = "" };
var r = await jxfw.Login(testLoginInfo);
logger.LogCritical("登录结果:{Result}", r); // 自行处理登录错误时的情况
var term = await jxfw.GetTerm();   // 获取学期
if (term is not null && await jxfw.GetLessons(term) is List<Lesson> lessons)
{
    // 以下是 GDUTSharp.Extra 的功能之一
    // 导出课程为 iCalendar 文件以便于导入其它日历程序中
    await File.WriteAllTextAsync("path/to/file",
        lessons.ToCalendarString(new ExtraExtensions.ICalConvertContext()
        {
            Alarm = new()
            {
                Trigger = new(new Duration(minutes: -30)),
                Action = AlarmAction.Display
            },
            IsMergeIfContinuous = true,
        }));
}
```
