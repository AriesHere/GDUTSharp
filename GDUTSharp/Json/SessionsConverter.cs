using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GDUTSharp.Json
{
    public class SessionsConverter : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return [];
            List<int> result = [];
            for (int i = 0; i < s.Length; i += 2)
            {
                result.Add(int.Parse(s[i..(i + 2)]));
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }

    public class SessionsConverter2 : JsonConverter<List<int>>
    {
        public override List<int> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return [];
            return [..s.Split(',').Select(int.Parse)];
        }

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
