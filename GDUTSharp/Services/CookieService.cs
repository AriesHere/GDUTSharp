using System.Net;
using GDUTSharp.Interfaces;

namespace GDUTSharp.Services
{
    public class CookieService : ICookieService
    {
        public CookieContainer Container { get; } = new();
    }
}
