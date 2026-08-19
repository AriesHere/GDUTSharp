using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using GDUTSharp.Shared.Type.DTO;
using GDUTSharp.Shared.Type;

namespace GDUTSharp.Shared.Json;

[JsonSourceGenerationOptions()]
[JsonSerializable(typeof(List<int>))]
[JsonSerializable(typeof(List<string>))]

[JsonSerializable(typeof(Lesson))]
[JsonSerializable(typeof(LessonDto))]
[JsonSerializable(typeof(LessonDtoCollection))]
[JsonSerializable(typeof(List<Lesson>))]

[JsonSerializable(typeof(ExamSchedule))]
[JsonSerializable(typeof(ExamScheduleDto))]
[JsonSerializable(typeof(ExamScheduleDtoCollection))]
[JsonSerializable(typeof(List<ExamSchedule>))]

[JsonSerializable(typeof(CourseScore))]
[JsonSerializable(typeof(CourseScoreDto))]
[JsonSerializable(typeof(CourseScoreDtoCollection))]
[JsonSerializable(typeof(List<CourseScore>))]

[JsonSerializable(typeof(CourseSel))]
[JsonSerializable(typeof(CourseSelDto))]
[JsonSerializable(typeof(CourseSelDtoCollection))]
[JsonSerializable(typeof(List<CourseSel>))]
public partial class AppJsonContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Encoder = JavaScriptEncoder.Create(
            UnicodeRanges.BasicLatin,
            UnicodeRanges.CjkUnifiedIdeographs,
            UnicodeRanges.CjkSymbolsandPunctuation),
    };

    public static AppJsonContext Context { get; } = new(DefaultOptions);
}
