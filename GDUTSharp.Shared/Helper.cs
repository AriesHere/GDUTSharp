using System.Text;
using System.Text.RegularExpressions;

namespace GDUTSharp.Shared;

public static partial class Helper
{
    extension(string s)
    {
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(s);

        public string Extract(string start, string end, out int currentIndex, int startIndex = 0)
        {
            var index = s.IndexOf(start, startIndex) + start.Length;
            var endIndex = s.IndexOf(end, index);
            currentIndex = endIndex;
            return s[index..endIndex];
        }

        public string Extract(string start, char end, out int currentIndex, int startIndex = 0)
        {
            var index = s.IndexOf(start, startIndex) + start.Length;
            var endIndex = s.IndexOf(end, index);
            currentIndex = endIndex;
            return s[index..endIndex];
        }
    }

    public static string GetString(this byte[] b) => Encoding.UTF8.GetString(b);

    public static Dictionary<TKey, TValue> AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out _))
        {
            dictionary.Add(key, value);
        }
        return dictionary;
    }

    [GeneratedRegex(@"<option\s+value=""(?<value>[^""]*)""\s*>(?<text>.*?)</option>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    public static partial Regex Notice_CategoriesRegex();
}
