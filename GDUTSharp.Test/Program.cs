using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Type;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GDUTSharp.Test
{
    public class DebugOptions
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddCommonClient(context.Configuration);
                    services.AddScoped<ICookieService, CookieService>();
                    services.AddScoped<IDataService, DataService>();
                    services.AddSingleton<ISecurityService, SecurityService>();

                    // Debug
                    services.Configure<DebugOptions>(context.Configuration.GetSection(nameof(DebugOptions)));
                })
                .Build();

            host.RunAsync();

            var debugConfig = host.Services.GetRequiredService<IOptions<DebugOptions>>();
            LoginInfo test = new() { UserName = debugConfig.Value.UserName, Password = debugConfig.Value.Password };

            var sc = host.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = sc.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var s = scope.ServiceProvider.GetRequiredService<IDataService>();

                var loginResult = s.Login(test);
                logger.LogInformation("登录结果:{Result}", loginResult);

                var authResult = s.Auth(Constant.UNDER_GRADUATE_LOGIN);
                logger.LogInformation("认证结果:{Result}", authResult);

                var result = s.GetExam("202502");
                if (result != null)
                {
                    foreach (var item in result.Lessons)
                    {
                        logger.LogInformation("{Result}", item);
                    }
                }
            }
        }
    }
}
