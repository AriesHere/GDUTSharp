using System.Net;
using System.Text.RegularExpressions;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class AuthService(ILogger<AuthService> logger, ICommonClient client, ISecurityService security) : IAuthService
{
    protected readonly ILogger<AuthService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly ISecurityService _security = security;

    public async virtual Task<HttpResponseMessage?> LoginAndAuth(IAuthService.SupportedServices? service = null, LoginInfo ? loginInfo = null)
    {
        try
        {
            var url = service switch
            {
                IAuthService.SupportedServices.JXFW => GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.UNDER_GRADUATE_LOGIN,
                IAuthService.SupportedServices.LIBRARY => GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.LIBRARY_LOGIN,
                _ => GDUTConstant.AUTHSERVER_LOGIN,
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            HttpResponseMessage response = await _client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // 需要登录
                if (loginInfo is null)
                {
                    if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证失败，尝试登录时未给出登录信息");
                    return null;
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("正在登录并认证");
                    var formData = new Dictionary<string, string>();
                    string pwdEncryptSalt = string.Empty;
                    string html = await response.Content.ReadAsStringAsync();
                    response.Dispose();

                    // 这里采用了相当激进的优化，如果校方改东西了，可能会出错。如果不希望这
                    // 样，请使用 GDUTSharp.Extra.SteadyAuthService 中的 LoginAndAuth 方法
                    Match saltMatch = Helper.Login_SaltRegex().Match(html);
                    pwdEncryptSalt = saltMatch.Success ? saltMatch.Groups[1].Value : "";
                    Match execMatch = Helper.Login_ExecRegex().Match(html);
                    string execution = execMatch.Success ? execMatch.Groups[1].Value : "";
                    formData["_eventId"] = "submit";
                    formData["cllt"] = "userNameLogin";
                    formData["dllt"] = "generalLogin";
                    formData["lt"] = "";
                    formData["execution"] = execution;
                    formData[""] = pwdEncryptSalt;
                    formData["username"] = loginInfo.UserName;
                    formData["password"] = _security.CbcEncrypt(loginInfo.Password, pwdEncryptSalt);

                    using var request2 = ICommonClient.CreateRequest(
                        HttpMethod.Post,
                        GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.UNDER_GRADUATE_LOGIN,
                        formData,
                        GDUTConstant.UNDER_GRADUATE_LOGIN);
                    response = await _client.SendAsync(request2);
                }
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("正在认证");
            }

            for (int i = 0; i < 5; i++)
            {
                if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.MovedPermanently)
                    break;
                string? location = response.Headers.Location?.AbsoluteUri;
                if (string.IsNullOrEmpty(location))
                    break;
                if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", i + 1, location);
                response.Dispose();
                using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                response = await _client.SendAsync(redirectRequest);
            }

            return response;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证或登录异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<bool> Logout()
    {
        try
        {
            using HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, GDUTConstant.AUTHSERVER_LOGOUT));
            var r = await response.Content.ReadAsStringAsync();
            return r.Contains("注销成功");
        }
        catch (CookieException e) when (e.Message.Contains("Domain") && e.Message.Contains("wisedu.com.cn"))
        {
            /// 预料中的异常（不过退出登录依然成功）：
            /// 退出登录失败 System.Net.CookieException: An error occurred when parsing the Cookie header for Uri 'https://authserver.gdut.edu.cn/authserver/logout'.
            ///        ---> System.Net.CookieException: The 'Domain'='wisedu.com.cn' part of the cookie is invalid.
            if (_logger.IsEnabled(LogLevel.Warning)) _logger.LogWarning("预料中的异常 {Exception}", e);
            return true;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("退出登录失败 {Exception}", e);
            return false;
        }
    }

    public async virtual Task<bool> CheckNeedCaptcha(string username)
    {
        try
        {
            using HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, GDUTConstant.AUTHSERVER_CHECK_CAPTCHA_PREFIX + username));
            var r = await response.Content.ReadAsStringAsync();
            if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("检查是否需要验证码返回的原始内容：\n{r}", r);
            // 返回内容： {"isNeed":false} 或 {"isNeed":true}
            // 为它专门写个类太麻烦了，就这样吧，索引越界什么的交给 try-catch 处理
            if (r[4] != 'N') throw new InvalidDataException($"返回内容为 {r} ，与预期不一致");
            return r[^3] == 'u';
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("检查是否需要验证码失败 {Exception}", e);
            return false;
        }
    }
}
