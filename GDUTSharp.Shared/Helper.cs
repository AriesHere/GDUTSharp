using System.Text;

namespace GDUTSharp.Shared
{
    public static class Helper
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
    }
}
