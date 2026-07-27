using System.Net;

namespace GDUTSharp.Interfaces;

public interface ICookieService
{
    CookieContainer Container { get; }
}
