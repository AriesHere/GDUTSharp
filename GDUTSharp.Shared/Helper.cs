using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace GDUTSharp.Shared;

public static partial class Helper
{
    extension(string s)
    {
        public byte[] ToBytes() => Encoding.UTF8.GetBytes(s);

        /// <summary>
        /// 从字符串中提取 <paramref name="start"/> 和 <paramref name="end"/> 之间的内容，并返回 <paramref name="end"/> 的索引
        /// </summary>
        public string Extract(string start, string end, out int currentIndex, int startIndex = 0)
        {
            var index = s.IndexOf(start, startIndex) + start.Length;
            var endIndex = s.IndexOf(end, index);
            currentIndex = endIndex + end.Length;
            return s[index..endIndex];
        }

        /// <summary>同 <see cref="extension(string).Extract(string, string, out int, int)"/></summary>
        public string Extract(string start, char end, out int currentIndex, int startIndex = 0)
        {
            var index = s.IndexOf(start, startIndex) + start.Length;
            var endIndex = s.IndexOf(end, index);
            currentIndex = endIndex + 1;
            return s[index..endIndex];
        }

        /// <summary>
        /// 从字符串中提取 <paramref name="start"/> 和 <paramref name="end"/> 之间的内容，并返回 <paramref name="end"/> 的索引
        /// </summary>
        public string LastExtract(string start, string end, out int currentIndex, int? startIndex = null)
        {
            Debug.WriteLine(s);
            var endIndex = s.LastIndexOf(end, startIndex ?? s.Length);
            Debug.WriteLine(endIndex);
            var index = s.LastIndexOf(start, endIndex) + start.Length;
            currentIndex = index;
            return s[index..endIndex];
        }
    }

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
