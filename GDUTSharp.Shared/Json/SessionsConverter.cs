using System.Text.Json;
using System.Text.Json.Serialization;

namespace GDUTSharp.Shared.Json
{
    /// <summary>
    /// 将 "010203" 转换为 [1,2,3]
    /// </summary>
    public class SessionsConverter : JsonConverter<List<int>>
    {
        public static List<int> Parse(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return [];
            List<int> result = [];
            for (int i = 0; i < s.Length; i += 2)
            {
                result.Add(int.Parse(s[i..(i + 2)]));
            }
            return result;
        }

        public override List<int> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
            Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// 将 "01,02,03" 转换为 [1,2,3]
    /// </summary>
    public class SessionsConverter2 : JsonConverter<List<int>>
    {
        public static List<int> Parse(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return [];
            return [.. s.Split(',').Select(int.Parse)];
        }

        public override List<int> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
            Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, List<int> value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
