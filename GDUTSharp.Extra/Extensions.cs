using System.Net;
using ClosedXML.Excel;
using GDUTSharp.Extra.Types;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using HtmlAgilityPack;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace GDUTSharp.Extra;

public static class Extensions
{
    // lesson
    extension(Lesson lesson)
    {
        /// <remaeks>部分信息不会写入（如学生人数、学期、班级名称）</remaeks>
        public List<CalendarEvent> ToCalendarEvent(ICalConvertOptions context)
        {
            List<CalendarEvent> result = [];
            bool useDate = lesson.Date is { };

            List<(TimeOnly, TimeOnly)> temp = [];
            if (context.IsMergeIfContinuous)
            {
                var groups = lesson.Sessions.SplitIntoConsecutiveGroups();
                foreach (var item in groups)
                    temp.Add(new(context.Sessions[item[0] - 1].StartTime, context.Sessions[item[^1] - 1].EndTime));
            }
            else foreach (var item in lesson.Sessions)
                temp.Add(new(context.Sessions[item - 1].StartTime, context.Sessions[item - 1].EndTime));

            foreach (var (start, end) in temp)
            {
                CalendarEvent c = new()
                {
                    Summary = lesson.Name,
                    Description = lesson.Profile,
                    Location = lesson.Location,
                };
                // time
                DateTime dtStart;
                DateTime dtEnd;
                if (useDate)
                {
                    dtStart = lesson.Date.ToDateTime(start);
                    dtEnd = lesson.Date.ToDateTime(end);
                }
                else
                {
                    if (context.StartDate is not null)
                    {
                        dtStart = context.StartDate.Value.ToDateTime(start).AddDays(lesson.Week * 7 + lesson.DayOfWeek - 1);
                        dtEnd = context.StartDate.Value.ToDateTime(end).AddDays(lesson.Week * 7 + lesson.DayOfWeek - 1);
                    }
                    else
                    {
                        throw new InvalidDataException("Unable to get DateTime");
                    }
                }
                c.Start = new(dtStart);
                c.End = new(dtEnd);

                if (context.Alarm is not null)
                {
                    c.Alarms.Add(context.Alarm);
                }
                result.Add(c);
            }
            return result;
        }
    }

    // List<Lesson>
    extension(List<Lesson> lessonList)
    {
        public Calendar ToCalendar(ICalConvertOptions context)
        {
            Calendar result = new();
            lessonList.ForEach(l => l.ToCalendarEvent(context).ForEach(result.Events.Add));
            return result;
        }

        public string? ToCalendarString(ICalConvertOptions context) =>
            new CalendarSerializer().SerializeToString(lessonList.ToCalendar(context));

        public async Task WriteAsICS(string path, ICalConvertOptions context) =>
            await File.WriteAllTextAsync(path, lessonList.ToCalendarString(context));

        /// <summary>
        /// 解析从教学服务中心导出的课程安排文件
        /// </summary>
        public static List<Lesson> Read(Stream stream, JXFWFileType type = JXFWFileType.AutoDetect)
        {
            using StreamReader reader = new(stream);
            List<Lesson> result = [];
            switch (type)
            {
                default:
                case JXFWFileType.AutoDetect:
                    var r = stream.ReadByte();
                    stream.Seek(0, SeekOrigin.Begin);
                    if (r is '"') goto case JXFWFileType.CSV;
                    if (r is '<') goto case JXFWFileType.XLS;
                    throw new FileLoadException("Could not detect the file type.");
                case JXFWFileType.CSV:
                case JXFWFileType.TEXT:
                    reader.ReadLine();  // Skip header
                    {
                        using StringReader temp = new(WebUtility.HtmlDecode(reader.ReadToEnd()));
                        while (temp.ReadLine() is string s)
                        {
                            s = s[1..^1];
                            var array = s.Split("\",\"");
                            PatternMatch([..array]);
                        }
                    }
                    break;
                case JXFWFileType.XLS:
                case JXFWFileType.DOC:
                    HtmlDocument doc = new();
                    doc.Load(stream);
                    var trNodes = doc.DocumentNode.SelectNodes("//tr");
                    if (trNodes != null)
                    {
                        foreach (var tr in trNodes)
                        {
                            var rowTds = tr.SelectNodes("./td");
                            if (rowTds != null)
                            {
                                List<string> rowList = [];
                                foreach (var td in rowTds)
                                {
                                    rowList.Add(td.InnerText.Trim());
                                }
                                PatternMatch(rowList);
                            }
                        }
                    }
                    break;
            }
            return result;

            void PatternMatch(List<string> values)
            {
                result.Add(values switch
                {
                    // 摘要：
                    // "课程名称","班级名称","人数","教师","周次","星期","节次","上课地点","排课日期","课序","类型","授课内容简介",
                    // "劳动教育","某某班级(1),某某班级(2)","60","某某老师","1","4","08","某上课地点","YYYY-MM-DD","1","实验教学",""
                    // "大学美育(1)","某某班级(1),某某班级(2),某某班级(3),某某班级(4)","98","某某老师","1","1","101112","某上课地点","YYYY-MM-DD","1","理论教学","简介内容"
                    [var lessonName, var className, var studentsCount, var teacher, var week, var dayOfWeek, var sessions, var location, var date, var classSequence, var lessonType, var profile] =>
                        new()
                        {
                            Name = lessonName,
                            ClassName = [.. className.Split(",")],
                            StudentsCount = int.Parse(studentsCount),
                            Teacher = teacher,
                            Week = int.Parse(week),
                            DayOfWeek = int.Parse(dayOfWeek),
                            Sessions = SessionsConverter.Parse(sessions),
                            Location = location,
                            Date = DateOnly.Parse(date),
                            LessonSequence = int.Parse(classSequence),
                            LessonType = lessonType,
                            Profile = profile,
                        },
                    _ => throw new ArgumentException("模式匹配失败"),
                });
            }
        }

        /// <summary>
        /// 导出为 xlsx 文件
        /// </summary>
        public void Export(string path, bool isAdjustToContents = true)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("课程数据");
            var header = new string[] {
                "学期",
                "课程名称",
                "教师",
                "教学班",
                "日期",
                "周次",
                "星期",
                "节次",
                "课序",
                "教学环节",
                "教学地点",
                "简介",};
            static IList<object> GetContent(Lesson l) =>
                [
                    l.Term.Name,
                    l.Name,
                    l.Teacher,
                    string.Join(',', l.ClassName),
                    l.Date,
                    l.Week,
                    l.DayOfWeek,
                    string.Join(',', l.Sessions),
                    l.LessonSequence,
                    l.LessonType,
                    l.Location,
                    l.Profile,
                ];
            ws.FillDataToWrokSheet<Lesson>(
                Helper.WSDesc,
                new(header, XLColor.LightBlue, true),
                new(GetContent, lessonList),
                isAdjustToContents,
                true,
                isAdjustToContents ? "SimSun" : null);
            wb.SaveAs(path);
        }
    }

    // ExamSchedule
    extension(ExamSchedule schedule)
    {
        public CalendarEvent ToCalendarEvent(ICalConvertOptions context)
        {
            DateTime dtStart = schedule.Date.ToDateTime(schedule.StartTime);
            DateTime dtEnd = schedule.Date.ToDateTime(schedule.EndTime);
            CalendarEvent c = new()
            {
                Summary = schedule.Name,
                Description = $"""
                课程名称：{schedule.Name}
                校区：{schedule.Campus}
                地点：{schedule.Location}
                监考老师：{schedule.Teachers}
                考试类别：{schedule.ExamType}
                安排类型：{schedule.ScheduleType}
                考试形式：{schedule.Format}
                试卷编号：{schedule.ExamPaperNumber}
                """,
                Location = schedule.Location,
                Start = new(dtStart),
                End = new(dtEnd),
            };
            if (context.Alarm is not null)
            {
                c.Alarms.Add(context.Alarm);
            }
            return c;
        }
    }

    // List<ExamSchedule>
    extension(List<ExamSchedule> scheduleList)
    {
        public Calendar ToCalendar(ICalConvertOptions context)
        {
            Calendar result = new();
            scheduleList.ForEach(schedule => result.Events.Add(schedule.ToCalendarEvent(context)));
            return result;
        }

        public string? ToCalendarString(ICalConvertOptions context) =>
            new CalendarSerializer().SerializeToString(scheduleList.ToCalendar(context));

        public async Task WriteAsICS(string path, ICalConvertOptions context) =>
            await File.WriteAllTextAsync(path, scheduleList.ToCalendarString(context));

        /// <summary>
        /// 导出为 xlsx 文件
        /// </summary>
        public void Export(string path, bool isAdjustToContents = true)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("考试安排数据");
            var header = new string[] {
                "课程名称",
                "校区",
                "地点",
                "监考老师",
                "日期",
                "周次",
                "星期",
                "开始时间",
                "结束时间",
                "考试类别",
                "安排类型",
                "考试形式",
                "试卷编号",};
            static IList<object> GetContent(ExamSchedule es) =>
                [
                    es.Name,
                    es.Campus,
                    es.Location,
                    string.Join(',', es.Teachers),
                    es.Date,
                    es.Week,
                    es.DayOfWeek,
                    es.StartTime,
                    es.EndTime,
                    es.ExamType,
                    es.ScheduleType,
                    es.Format,
                    es.ExamPaperNumber,
                ];
            ws.FillDataToWrokSheet<ExamSchedule>(
                Helper.WSDesc,
                new(header, XLColor.LightBlue, true),
                new(GetContent, scheduleList),
                isAdjustToContents,
                true,
                isAdjustToContents ? "SimSun" : null);
            wb.SaveAs(path);
        }
    }

    // List<CourseSel>
    extension(List<CourseSel> courseSels)
    {
        /// <summary>
        /// 导出为 xlsx 文件
        /// </summary>
        public void Export(string path, bool isAdjustToContents = true)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("选课数据");
            var header = new string[] {
                "课程任务代码",
                "课程大类",
                "课程分类",
                "培养项目名称",
                "课程名称",
                "学时",
                "学分",
                "教师",
                "排课人数",
                "已选人数",};
            static IList<object> GetContent(CourseSel cs) =>
                [
                    cs.CourseCode,
                    cs.Type,
                    cs.Category,
                    cs.ProgramName,
                    cs.Name,
                    cs.ClassHour,
                    cs.Credit,
                    cs.Teacher,
                    cs.StudentsCount,
                    cs.EnrolledCount
                ];
            ws.FillDataToWrokSheet<CourseSel>(
                Helper.WSDesc,
                new(header, XLColor.LightBlue, true),
                new(GetContent, courseSels),
                isAdjustToContents,
                true,
                isAdjustToContents ? "SimSun" : null);
            wb.SaveAs(path);
        }
    }

    // IDataService
    // 这里是不适合放在 IJXFWService 接口中的扩展方法，因为它们依赖于接口的实现类的具体行为
    extension(IJXFWService dataService)
    {
        /// <summary>
        /// 获取指定年份的学分绩点和学业成绩平均分，注意检查两个学期的课程成绩是否都获取成功（即 term1Count 和 term2Count 均正常）
        /// </summary>
        /// <param name="year">
        /// 四位数字的年份，填秋季的那年，如某一学年的上学期为2025秋季，下学期为2026春季，則 year 应为 "2025"
        /// </param>
        public async Task<(int term1Count, int term2Count, float GPA, float averageGrade)> GetGPAAndAverageGrade(int year)
        {
            Term term = new(year, TermPeriod.First);
            var scores1 = await dataService.GetCourseScore(term);
            term = term.Next();
            var scores2 = await dataService.GetCourseScore(term);
            if (scores1 is null || scores2 is null)
            {
                throw new NullReferenceException("课程成绩获取异常");
            }
            List<CourseScore> courseScores = [.. scores1, .. scores2];
            var gpa = courseScores.GetGPA();
            var averageGrade = courseScores.GetAverageGrade();
            return (scores1.Count, scores2.Count, gpa, averageGrade);
        }
    }

    public enum JXFWFileType
    {
        AutoDetect,
        // 以下四种是教学管理系统上显示的支持导出的格式，然而实际上有几种会导出
        // 完全相同的文件。因此我们将 DOC 视作 XLS，把 TEXT 视作 CSV 进行处理。
        XLS,
        DOC,
        CSV,
        TEXT,
    }
}
