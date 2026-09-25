using GDUTSharp.Shared.Type;

namespace GDUTSharp.Interfaces;

/// <summary>体质测试系统</summary>
public interface ISportsTestService
{
    /// <returns>GIF89a 文件</returns>
    public Task<byte[]?> GetCaptcha();

    /// <remarks>
    /// 不使用统一认证中心认证服务，使用自己的账号密码<br/>
    /// 请先调用 <see cref="GetCaptcha"/>，将验证码填入 <paramref name="loginInfo"/>
    /// 的 Chaptcha 属性中，再调用本方法。
    /// </remarks>
    public Task<bool> Login(LoginInfo loginInfo);
}
