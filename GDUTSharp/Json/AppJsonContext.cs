using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using GDUTSharp.Type;

namespace GDUTSharp.Json;

[JsonSourceGenerationOptions()]
[JsonSerializable(typeof(Lesson))]
[JsonSerializable(typeof(LessonCollection))]
[JsonSerializable(typeof(Exam))]
[JsonSerializable(typeof(ExamCollection))]
[JsonSerializable(typeof(LessonScore))]
[JsonSerializable(typeof(LessonScoreCollection))]
[JsonSerializable(typeof(CourseSelection))]
[JsonSerializable(typeof(CourseSelCollection))]
[JsonSerializable(typeof(List<CourseSelection>))]
[JsonSerializable(typeof(List<int>))]
[JsonSerializable(typeof(List<string>))]
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
