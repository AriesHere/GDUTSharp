using GDUTSharp.Shared.Type;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace GDUTSharp.Extra;

public static class Extensions
{
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
