using System.Text;
using System.Text.RegularExpressions;

namespace GDUTSharp.Shared;

public static partial class Helper
{
    public static byte[] ToBytes(this string s) => Encoding.UTF8.GetBytes(s);

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
