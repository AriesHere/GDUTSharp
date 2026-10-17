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
        public List<CalendarEvent> ToCalendarEvent(ICalConvertOptions<Lesson> opt)
        {
            List<CalendarEvent> result = [];
            List<(TimeOnly, TimeOnly)> temp = [];
            if (opt.IsMergeIfContinuous)
                temp.Add(new(opt.Sessions[lesson.Sessions[0] - 1].StartTime, opt.Sessions[lesson.Sessions[0] - 1].EndTime));
            else foreach (var item in lesson.Sessions)
                temp.Add(new(opt.Sessions[item - 1].StartTime, opt.Sessions[item - 1].EndTime));

            foreach (var (start, end) in temp)
            {
                CalendarEvent c = new()
                {
                    Summary = lesson.Name,
                    Description = opt.GetDescriptionFunc.Invoke(lesson),
                    Location = lesson.Location,
                    Categories = opt.Categories,
                };
                // time
                var date = lesson.Date == DateOnly.MinValue
                    ? opt.StartDate ?? throw new InvalidDataException("Unable to get DateTime")
                    : lesson.Date;
                var offset = lesson.Date == DateOnly.MinValue ? lesson.Week * 7 + lesson.DayOfWeek - 1 : 0;
                c.Start = new(date.ToDateTime(start).AddDays(offset));
                c.End = new(date.ToDateTime(end).AddDays(offset));

                if (opt.GetAlarm(lesson) is { } alarm)
                {
                    c.Alarms.Add(alarm);
                }
                result.Add(c);
            }
            return result;
        }
    }

    // IEnumerable<Lesson>
    extension(IEnumerable<Lesson> lessons)
    {
        public Calendar ToCalendar(ICalConvertOptions<Lesson> opt)
        {
            Calendar result = new();
            var lessonsData = opt.IsReverseOrder ? lessons.Reverse() : lessons;
            foreach (var item in lessonsData)
            {
                item.ToCalendarEvent(opt).ForEach(result.Events.Add);
            }
            return result;
        }

        public string? ToCalendarString(ICalConvertOptions<Lesson> opt) =>
            new CalendarSerializer().SerializeToString(lessons.ToCalendar(opt));

        public async Task WriteAsICS(string path, ICalConvertOptions<Lesson> opt) =>
            await File.WriteAllTextAsync(path, lessons.ToCalendarString(opt));

        /// <summary>
        /// 解析从教学服务中心导出的课程安排文件
        /// </summary>
        public static List<Lesson> Read(Stream stream, LessonsFileType type = LessonsFileType.AutoDetect)
        {
            using StreamReader reader = new(stream);
            List<Lesson> result = [];
            switch (type)
            {
                default:
                case LessonsFileType.AutoDetect:
                    var r = stream.ReadByte();
                    stream.Seek(0, SeekOrigin.Begin);
                    if (r is '"') goto case LessonsFileType.CSV;
                    if (r is '<') goto case LessonsFileType.XLS;
                    throw new FileLoadException("Could not detect the file type.");
                case LessonsFileType.CSV:
                case LessonsFileType.TEXT:
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
                case LessonsFileType.XLS:
                case LessonsFileType.DOC:
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
                new(GetContent, lessons),
                isAdjustToContents,
                true,
                isAdjustToContents ? "SimSun" : null);
            wb.SaveAs(path);
        }
    }

    // ExamSchedule
    extension(ExamSchedule schedule)
    {
        public CalendarEvent ToCalendarEvent(ICalConvertOptions<ExamSchedule> opt)
        {
            DateTime dtStart = schedule.Date.ToDateTime(schedule.StartTime);
            DateTime dtEnd = schedule.Date.ToDateTime(schedule.EndTime);
            CalendarEvent c = new()
            {
                Summary = schedule.Name,
                Description = opt.GetDescriptionFunc.Invoke(schedule),
                Location = schedule.Location,
                Categories = opt.Categories,
                Start = new(dtStart),
                End = new(dtEnd),
            };
            if (opt.GetAlarm(schedule) is { } alarm)
            {
                c.Alarms.Add(alarm);
            }
            return c;
        }
    }

    // IEnumerable<ExamSchedule>
    extension(IEnumerable<ExamSchedule> schedules)
    {
        public Calendar ToCalendar(ICalConvertOptions<ExamSchedule> opt)
        {
            Calendar result = new();
            var scheduleData = opt.IsReverseOrder ? schedules.Reverse() : schedules;
            foreach (var item in scheduleData)
            {
                result.Events.Add(item.ToCalendarEvent(opt));
            }
            return result;
        }

        public string? ToCalendarString(ICalConvertOptions<ExamSchedule> opt) =>
            new CalendarSerializer().SerializeToString(schedules.ToCalendar(opt));

        public async Task WriteAsICS(string path, ICalConvertOptions<ExamSchedule> opt) =>
            await File.WriteAllTextAsync(path, schedules.ToCalendarString(opt));

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
            static IEnumerable<object> GetContent(ExamSchedule es) =>
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
                new(GetContent, schedules),
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

    // IAcademicAffairsService
    // 这里是不适合放在 IAcademicAffairsService 接口中的扩展方法，因为它们依赖于接口的实现类的具体行为
    extension(IAcademicAffairsService services)
    {
        /// <summary>
        /// 获取指定年份的学分绩点和学业成绩平均分，注意检查两个学期的课程成绩是否都获取成功
        /// </summary>
        /// <param name="year">
        /// 四位数字的年份，填秋季的那年，如某一学年的上学期为2025秋季，下学期为2026春季，則 year 应为 "2025"
        /// </param>
        /// <returns>
        /// gpa 是平均学分绩点，范围在 0.0-5.0，averageGrade 是平均学业成绩，范围在 0.0-100.0
        /// </returns>
        public async Task<(List<CourseScore> term1Score, List<CourseScore> term2Score, float gpa, float averageGrade)> GetGPAAndAverageGrade(CookieContainer cookies, int year, CancellationToken token = default)
        {
            Term term = new(year, TermPeriod.First);
            var scores1 = await services.GetCourseScore(cookies, term, token);
            ArgumentNullException.ThrowIfNull(scores1, nameof(scores1));
            term.Period = TermPeriod.Second;
            var scores2 = await services.GetCourseScore(cookies, term, token);
            ArgumentNullException.ThrowIfNull(scores2, nameof(scores2));
            List<CourseScore> courseScores = [.. scores1, .. scores2];
            var gpa = courseScores.GetGPA();
            var averageGrade = courseScores.GetAverageGrade();
            return (scores1, scores2, gpa, averageGrade);
        }
    }
}
