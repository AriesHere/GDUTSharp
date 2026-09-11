using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

// TODO: 获取验证码

/// <summary>
/// 统一认证中心认证服务接口
/// </summary>
/// <remarks>
/// 仅支持统一认证中心认证的系统可以使用本接口进行认证。
/// </remarks>
public interface IAuthService
{
    /// <remarks>
    /// 通过返回内容自行判断是否登录成功（对于不同系统，判断方式不同）。
    /// 对于同一用户，无需反复登录。对于同一用户在同一系统的操作，无需反复认证。
    /// </remarks>
    public Task<HttpResponseMessage?> LoginAndAuth(SupportedServices? service = null, LoginInfo ? loginInfo = null);

    /// <remarks>
    /// 注意：即使未登录，调用本方法也会返回 true，因为统一认证中心的登出操作是幂等的
    /// </remarks>
    public Task<bool> Logout();

    /// <summary>
    /// 检查是否需要验证码
    /// </summary>
    /// <param name="username">学号</param>
    public Task<bool> CheckNeedCaptcha(string username);

    public enum SupportedServices
    {
        JXFW,       // 教学服务系统
        LIBRARY,    // 图书馆
    }
}
