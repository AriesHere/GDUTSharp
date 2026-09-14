using System.Text;
using System.Text.RegularExpressions;

namespace GDUTSharp.Shared;

public static partial class Helper
{
    public static byte[] GetBytes(this string s) => Encoding.UTF8.GetBytes(s);
    public static string GetString(this byte[] b) => Encoding.UTF8.GetString(b);
    public static int TermStringToInt6Digit(string term)
    {
        // 2026春季 => 202502
        // 2026秋季 => 202601
        int result = int.Parse(term[..4]) * 100;
        if (term.Contains('春'))
        {
            result -= 100;
            result += 2;
        }
        else if (term.Contains('秋'))
        {
            result += 1;
        }
        else
        {
            throw new ArgumentException($"Invalid term: {term}");
        }
        return result;
    }

    [GeneratedRegex(@"id=""pwdEncryptSalt""[^>]*?value=""([^""]*)""")]
    public static partial Regex Login_SaltRegex();

    [GeneratedRegex(@"id=""execution""[^>]*?value=""([^""]*)""")]
    public static partial Regex Login_ExecRegex();

    [GeneratedRegex(@"<option\s+value=""(?<value>[^""]*)""\s*>(?<text>.*?)</option>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    public static partial Regex Notice_CategoriesRegex();
}
