using Microsoft.Extensions.Logging;

namespace GDUTSharp.Shared;

public static partial class Log
{
    /// <summary>Http 请求异常</summary>
    [LoggerMessage(LogLevel.Error, "请求 {Uri} 第 {Attempt} 次尝试抛出异常。")]
    public static partial void HttpRequestError(ILogger logger, Uri? uri, int attempt, Exception? ex);

    /// <summary>Http 请求异常</summary>
    [LoggerMessage(LogLevel.Error, "请求 {Uri} 第 {Attempt} 次尝试超时。")]
    public static partial void HttpRequestTimeout(ILogger logger, Uri? uri, int attempt, Exception? ex);

    /// <summary>登录失败</summary>
    [LoggerMessage(LogLevel.Error, "登录失败。")]
    public static partial void LoginFailed(ILogger logger, Exception? ex);

    /// <summary>验证码错误失败</summary>
    [LoggerMessage(LogLevel.Error, "验证码错误。")]
    public static partial void CaptchaFailed(ILogger logger, Exception? ex);

    /// <summary>尝试 xxx 时抛出异常</summary>
    [LoggerMessage(LogLevel.Error, "尝试 {Target} 时抛出异常。")]
    public static partial void TryFailed(ILogger logger, string? target, Exception? ex);

    /// <summary>未登录或登录状态失效</summary>
    [LoggerMessage(LogLevel.Error, "未登录或登录状态失效")]
    public static partial void LoginStatusInvalid(ILogger logger);

    /// <summary>忽略非法 Set-Cookie</summary>
    [LoggerMessage(LogLevel.Warning, "忽略非法 Set-Cookie: {Cookie}")]
    public static partial void IgnoreInvalidSetCookie(ILogger logger, string? cookie, Exception? ex);

    /// <summary>正在登录并认证</summary>
    [LoggerMessage(LogLevel.Information, "正在登录并认证")]
    public static partial void LoginAndAuth(ILogger logger);

    /// <summary>正在登录</summary>
    [LoggerMessage(LogLevel.Information, "正在登录")]
    public static partial void Login(ILogger logger);
}
