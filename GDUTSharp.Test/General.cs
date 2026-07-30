using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GDUTSharp.Test
{
    [TestClass]
    public static class General
    {
        public static IHost? Host;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            DIInit();
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup(TestContext context)
        {
            if (Host != null)
            {
                await Host.StopAsync();
            }
        }

        private static void DIInit()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddCommonClient(context.Configuration);
                    services.AddScoped<ICookieService, CookieService>();
                    services.AddScoped<IDataService, DataService>();
                    services.AddSingleton<ISecurityService, SecurityService>();
                    services.Configure<DebugOptions>(context.Configuration.GetSection(nameof(DebugOptions)));
                })
                .Build();

            Host.RunAsync();
        }
    }
}
