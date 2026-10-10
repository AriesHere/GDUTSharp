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
    - 教学计划（又称学习计划）
    - 学期注册信息
    - 课程任务
- 图书馆
    - 每日推荐
    - 借阅信息
- 通知公文网
    - 通知数据
- 体质测试平台
    - 体测成绩
- 其它
    - 绩点计算

GDUTSharp.Extra:
- 为部分 Service 提供更稳健的实现
- 读取直接从教学服务中心导出的数据
- 将课表信息和考试安排信息输出为 [iCalendar](https://icalendar.org/) 文件
- 将课程数据，考试安排数据、课程任务数据和选课数据导出为 xlsx 文件

## 使用示例  
```C#
IHost AppHost = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<INoticeService, NoticeService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ILibraryService, LibraryService>();
        services.AddSingleton<IJXFWService, JXFWService>();
        services.AddSingleton<ISportsTestService, SportsTestService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddCommonClient(context.Configuration);
    })
    .Build();
AppHost.Start();

LoginInfo testLoginInfo = new() { UserName = "", Password = "" };
var logger = AppHost.Services.GetRequiredService<ILogger<Program>>();
var jxfw = AppHost.Services.GetRequiredService<IJXFWService>();
var cookies = new CookieContainer();
var r = await jxfw.Login(cookies, testLoginInfo); // 自行处理登录失败时的情况
logger.LogCritical("登录结果:{Result}", r);
var term = jxfw.GetTerm(cookies).Result;    // 获取学期
if (term is not null && await jxfw.GetLessons(cookies, term) is List<Lesson> lessons)
{
    // 以下是 GDUTSharp.Extra 的功能之一：导出课程为 iCalendar 文件以便于导入其它日历程序中
    var opt = new ICalConvertOptions()
    {
        Alarm = new()
        {
            Trigger = new(new Duration(minutes: -20)),
            Action = AlarmAction.Display,
        },
        IsMergeIfContinuous = true,
    };
    // 为早八设置特殊提醒（会覆盖上文的 alarm），返回 null 则不设置提醒
    opt.SetAlarmFunc += (lesson) => lesson.Sessions.Any(i => i == 1) ? null : new()
    {
        Trigger = new(new Duration(minutes: -40)),
        Action = AlarmAction.Display,
    };
    await File.WriteAllTextAsync("path/to/file", lessons.ToCalendarString(opt));
}
```
