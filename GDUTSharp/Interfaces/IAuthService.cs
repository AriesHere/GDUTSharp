using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// 统一认证中心登录，必须先后执行 <see cref="Login(LoginInfo)"/> 和 <see cref="Auth(string)"/> 之后才能进行其它操作。
    /// </summary>
    /// <remarks>
    /// 返回 ture 即表示登录成功。
    /// 对于同一用户，无需反复登录。对于同一用户在同一系统的操作，无需反复认证。
    /// </remarks>
    public Task<bool> Login(LoginInfo user);

    /// <remarks>
    /// 注意：即使未登录，调用本方法也会返回 true，因为统一认证中心的登出操作是幂等的
    /// </remarks>
    public Task<bool> Logout();

    /// <summary>
    /// 通过统一认证中心认证
    /// </summary>
    public Task<bool> Auth(SupportedServices service);

    public enum SupportedServices
    {
        JXFW,       // 教学服务系统
        LIBRARY,    // 图书馆
    }
}
