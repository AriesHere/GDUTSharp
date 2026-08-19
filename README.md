# GDUTSharp

对 [gdutday/gdutday-wechat3.0-java](https://github.com/gdutday/gdutday-wechat3.0-java) 的 C# 不完全重实现

目前仅支持本科生相关的部分

支持 AOT

示例:

```C#
// 注册到 DI 容器中
var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // 使用此方法以自动完成对CommonClient的所有配置
        services.AddCommonClient(context.Configuration);
        
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<IDataService, DataService>();
        services.AddSingleton<ISecurityService, SecurityService>();
    })
    .Build();

host.RunAsync();

// 这里填充学号和密码
LoginInfo test = new() { UserName = "", Password = "" };

var sc = host.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = sc.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

    // 先登录，再认证
    // 如果学号密码正确，这两个结果都是 true
    // 自行补充学号密码错误时的处理逻辑
    var loginResult = dataService.Login(test);
    logger.LogInformation("登录结果:{Result}", loginResult);

    var authResult = dataService.Auth(Constant.UNDER_GRADUATE_LOGIN);
    logger.LogInformation("认证结果:{Result}", authResult);

    // 这里可以先通过 GetTerm 方法获取学期代码
    var result = dataService.GetExamSchedule("202502");
    if (result != null)
    {
        foreach (var item in result.Lessons)
        {
            // 输出
            logger.LogInformation("{Result}", item);
        }
    }
}
```
