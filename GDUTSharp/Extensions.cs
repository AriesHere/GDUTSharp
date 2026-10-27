using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GDUTSharp;

public static partial class Extensions
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

    public static partial IServiceCollection AddGDUTSharp(this IServiceCollection services, IConfiguration configuration);
}
