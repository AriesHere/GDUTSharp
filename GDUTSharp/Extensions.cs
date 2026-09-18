using GDUTSharp.Interfaces;
using GDUTSharp.Shared.Type;

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
}
