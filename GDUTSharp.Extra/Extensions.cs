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
        public List<CalendarEvent> ToCalendarEvent(SessionCollection? sessions = null, Alarm? alarm = null, DateOnly? startDate = null, string[]? categories = null)
        {
            List<CalendarEvent> result = [];
            SessionCollection s = sessions ?? SessionCollection.Default;
            bool useDate = lesson.Date is { };
            foreach (var item in lesson.Sessions)
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
                    dtStart = lesson.Date.ToDateTime(s[item - 1].StartTime);
                    dtEnd = lesson.Date.ToDateTime(s[item - 1].EndTime);
                }
                else
                {
                    if (startDate is not null)
                    {
                        dtStart = startDate.Value.ToDateTime(s[item - 1].StartTime).AddDays(lesson.Week * 7 + lesson.DayOfWeek - 1);
                        dtEnd = startDate.Value.ToDateTime(s[item - 1].EndTime).AddDays(lesson.Week * 7 + lesson.DayOfWeek - 1);
                    }
                    else
                    {
                        throw new InvalidDataException("Unable to get DateTime");
                    }
                }
                c.Start = new(dtStart);
                c.End = new(dtEnd);

                if (alarm is not null)
                {
                    c.Alarms.Add(alarm);
                }
                result.Add(c);
            }
            return result;
        }
    }

    extension(List<Lesson> lessonList)
    {
        public Calendar ToCalendar(SessionCollection? sessions = null, Alarm? alarm = null, DateOnly? startDate = null, string[]? categories = null)
        {
            Calendar result = new();
            foreach (var item in lessonList)
            {
                item.ToCalendarEvent(sessions, alarm, startDate, categories).ForEach(result.Events.Add);
            }
            return result;
        }

        public string? ToCalendarString(SessionCollection? sessions = null, Alarm? alarm = null, DateOnly? startDate = null, string[]? categories = null)
        {
            var serializer = new CalendarSerializer();
            return serializer.SerializeToString(lessonList.ToCalendar(sessions, alarm, startDate, categories));
        }

        public async Task WriteAsICS(string path) => await File.WriteAllTextAsync(path, lessonList.ToCalendarString());
    }
}
