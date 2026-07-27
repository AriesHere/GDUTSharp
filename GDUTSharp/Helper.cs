using System.Text;

namespace GDUTSharp
{
    public static class Helper
    {
        public static byte[] GetBytes(this string s) => Encoding.UTF8.GetBytes(s);
        public static string GetString(this byte[] b) => Encoding.UTF8.GetString(b);
    }
}
