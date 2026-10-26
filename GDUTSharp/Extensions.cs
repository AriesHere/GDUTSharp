using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GDUTSharp;

public static class Extensions
{
    // List<CourseScore>
    extension(List<CourseScore> scores)
    {
        /// <summary>平均学分绩点</summary>
        /// <remarks>
        /// 参照于 2021-10-28 发布的《广东工业大学全日制本科学生综合素质测评实施办法》设计
        /// </remarks>
        public float GetGPA() => scores.Sum(x => x.Gp * x.Credit) / scores.Sum(x => x.Credit);

        /// <summary>学业成绩平均分</summary>
        /// <remarks>
        /// 参照于 2021-10-28 发布的《广东工业大学全日制本科学生综合素质测评实施办法》设计
        /// </remarks>
        public float GetAverageGrade() => scores.GetGPA() * 10 + 50;
    }

    // IServiceCollection
    extension(IServiceCollection services)
    {
        /// <summary>将所有 GDUTSharp 服务以单例模式注册到 <see cref="IServiceCollection"/> 中</summary>
        /// <remarks>
        /// <para>
        /// <see cref="CommonClient"/> 的注册方式由 <see cref="CommonClientExtensions.AddCommonClient(IServiceCollection, IConfiguration)"/> 实现
        /// </para>
        /// TODO: 转为通过源生成器实现
        /// </remarks>
        public IServiceCollection AddGDUTSharp(IConfiguration configuration)
        {
            services.AddSingleton<ISecurityService, AesCbcService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IAcademicAffairsService, AcademicAffairsService>();
            services.AddSingleton<ILibraryService, LibraryService>();
            services.AddSingleton<INoticeService, NoticeService>();
            services.AddSingleton<ISportsTestService, SportsTestService>();
            services.AddSingleton<IEhallService, EhallService>();
            services.AddCommonClient(configuration);
            return services;
        }
    }
}
