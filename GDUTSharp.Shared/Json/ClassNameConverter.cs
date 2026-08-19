using System.Text.Json;
using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Json
{
    public class ClassNameConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return [];
            return [.. s.Split(',')];
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
