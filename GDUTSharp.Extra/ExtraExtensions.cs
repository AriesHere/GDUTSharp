using System.Net;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using HtmlAgilityPack;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace GDUTSharp.Extra;

public static class ExtraExtensions
{
    // lesson
    extension(Lesson lesson)
    {
        /// <remaeks>
        /// 部分信息不会写入（如学生人数、学期、班级名称）
        /// </remaeks>
        /// <param name="startDate">
        ///     如果 <paramref name="startDate"/> 为 null，请确保 lesson 数据中的 Date 为有效值。
        ///     如果 <paramref name="startDate"/> 不为 null，请确保它为星期一，否则日期推断会出问题。
        /// </param>
        public List<CalendarEvent> ToCalendarEvent(ICalConvertContext context)
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
        public Calendar ToCalendar(ICalConvertContext context)
        {
            Calendar result = new();
            foreach (var item in lessonList)
            {
                item.ToCalendarEvent(context).ForEach(result.Events.Add);
            }
            return result;
        }

        public string? ToCalendarString(ICalConvertContext context)
        {
            var serializer = new CalendarSerializer();
            return serializer.SerializeToString(lessonList.ToCalendar(context));
        }

        public async Task WriteAsICS(string path, ICalConvertContext context) => await File.WriteAllTextAsync(path, lessonList.ToCalendarString(context));

        /// <summary>
        /// 解析从教学服务中心导出的课程安排文件
        /// </summary>
        public static List<Lesson> Read(string path, JXFWFileType type = JXFWFileType.AutoDetect)
        {
            using FileStream fs = new(path, FileMode.Open);
            using StreamReader reader = new(fs);
            List<Lesson> result = [];
            SWITCH: switch (type)
            {
                case JXFWFileType.AutoDetect:
                    type = fs.ReadByte() switch
                    {
                        '\"' => JXFWFileType.XLS,
                        '<' => JXFWFileType.CSV,
                        _ => throw new FileLoadException("Could not detect the file type."),
                    };
                    fs.Seek(0, SeekOrigin.Begin);
                    goto SWITCH;
                case JXFWFileType.XLS:
                case JXFWFileType.DOC:
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
                case JXFWFileType.CSV:
                case JXFWFileType.TEXT:
                    HtmlDocument doc = new();
                    doc.Load(fs);
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
                default:
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
                    _ => throw new ArgumentException("Pattern match failed."),
                });
            }
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

    public class ICalConvertContext
    {
        public SessionCollection Sessions = SessionCollection.Default;

        /// <summary>
        /// 是否在 <see cref="Sessions"/> 连续时自动合并时间
        /// </summary>
        public bool IsMergeIfContinuous = true;

        public Alarm? Alarm = null;

        /// <summary>
        /// 首周星期一的日期
        /// </summary>
        public DateOnly? StartDate = null;

        public List<string> Categories = [];
    }
}
