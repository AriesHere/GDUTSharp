using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Json;

public class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    protected readonly string[] formats = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm" };

    public override DateTime Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
    {
        var t = reader.GetString();
        if (t is null)
        {
            return DateTime.MinValue;
        }
        return DateTime.ParseExact(t, formats, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}