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

    // IDataService
    // 这里是不适合放在 IDataService 接口中的扩展方法，因为它们依赖于接口的实现类的具体行为
    extension(IDataService dataService)
    {
        /// <summary>
        /// 获取指定年份的学分绩点和学业成绩平均分，注意检查两个学期的课程成绩是否都获取成功（即 term1Count 和 term2Count 均正常）
        /// </summary>
        /// <param name="year">
        /// 四位数字的年份，填秋季的那年，如某一学年的上学期为2025秋季，下学期为2026春季，則 year 应为 "2025"
        /// </param>
        public async Task<(int term1Count, int term2Count, float GPA, float averageGrade)> GetGPAAndAverageGrade(string year)
        {
            if (year.Length != 4 || year.Any(x => !char.IsDigit(x)))
            {
                throw new ArgumentException("年份格式不正确", nameof(year));
            }
            var term1 = $"{year}01";
            var scores1 = await dataService.GetCourseScore(term1);
            var term2 = $"{year}02";
            var scores2 = await dataService.GetCourseScore(term2);
            if (scores1 is null || scores2 is null)
            {
                throw new NullReferenceException("课程成绩获取异常");
            }
            List<CourseScore> courseScores = [..scores1, ..scores2];
            var gpa = courseScores.GetGPA();
            var averageGrade = courseScores.GetAverageGrade();
            return (scores1.Count, scores2.Count, gpa, averageGrade);
        }
    }
}
