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

    /// <summary>获取可用的年份（即应当有体测成绩的年份）</summary>
    /// <remarks>
    /// 成绩录入需要时间，因此最新一年可能需要过一段时间才能通过 <see cref="GetScore(string)"/>
    /// 获得有效数据，但它仍会被包含在返回结果中
    /// </remarks>
    public Task<List<string>?> GetYears();

    /// <summary>获取特定年份的体测成绩</summary>
    /// <param name="year">四位数年份，如“2026”</param>
    public Task<SportsTestScore?> GetScore(string year);
}
