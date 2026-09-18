using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class AuthService(ILogger<AuthService> logger, ICommonClient client, ISecurityService security) : IAuthService
{
    protected readonly ILogger<AuthService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly ISecurityService _security = security;

    /// <remarks>TODO: 不知为何，使用随机生成的 iv 有概率登录/认证失败</remarks>
    protected readonly byte[] INIT_VECTOR = "Jisniwqjwqjwqjww".ToBytes();

    /// <summary>附加前缀</summary>
    /// <remarks>TODO: 不知为何，使用随机前缀时会出问题</remarks>
    protected virtual byte[] PrefixProcess(string raw) => [.."J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1".ToBytes(), ..raw.ToBytes()];

    public async virtual Task<HttpResponseMessage?> LoginAndAuth(IAuthService.SupportedServices? service = null, LoginInfo ? loginInfo = null)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            var url = service switch
            {
                IAuthService.SupportedServices.JXFW => GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.UNDER_GRADUATE_LOGIN,
                IAuthService.SupportedServices.LIBRARY => GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.LIBRARY_LOGIN,
                _ => GDUTConstant.AUTHSERVER_LOGIN,
            };
            request = new HttpRequestMessage(HttpMethod.Post, url);
            response = await _client.SendAsync(request);
            request.Dispose();

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
                    formData["password"] = Convert.ToBase64String(
                        _security.AesCbcEncrypt(this.PrefixProcess(loginInfo.Password), pwdEncryptSalt.ToBytes(), INIT_VECTOR));

                    request = ICommonClient.CreateRequest(
                        HttpMethod.Post,
                        GDUTConstant.AUTHSERVER_AUTH_PREFIX + GDUTConstant.UNDER_GRADUATE_LOGIN,
                        formData,
                        GDUTConstant.UNDER_GRADUATE_LOGIN);
                    response = await _client.SendAsync(request);
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
                request = new HttpRequestMessage(HttpMethod.Get, location);
                response = await _client.SendAsync(request);
                request.Dispose();
            }

            return response;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证或登录异常。 {Exception}", e);
            return null;
        }
        finally
        {
            request?.Dispose();
        }
    }

    public async virtual Task<bool> Logout()
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GDUTConstant.AUTHSERVER_LOGOUT);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var r = await response.Content.ReadAsStringAsync();
            return r.Contains("注销成功");
        }
        catch (CookieException e) when (e.Message.Contains("Domain") && e.Message.Contains("wisedu.com.cn"))
        {
            // 预料中的异常（不过退出登录依然成功）：
            // 退出登录失败 System.Net.CookieException: An error occurred when parsing the Cookie header for Uri 'https://authserver.gdut.edu.cn/authserver/logout'.
            //        ---> System.Net.CookieException: The 'Domain'='wisedu.com.cn' part of the cookie is invalid.
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
            using HttpRequestMessage request = new(HttpMethod.Get, GDUTConstant.AUTHSERVER_CHECK_CAPTCHA_PREFIX + username);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var r = await response.Content.ReadAsStringAsync();
            return r.Contains("true");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("检查是否需要验证码失败 {Exception}", e);
            return false;
        }
    }

    public async virtual Task<AuthServerCaptcha?> GetCaptcha()
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GDUTConstant.AUTHSERVER_CAPTCHA_GET);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var r = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.AuthServerCaptchaDto);
            if (r is null) return null;
            return r;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("获取验证码失败 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<bool> SubmitCaptcha(SliderPayloadDto payload, AuthServerCaptcha captcha)
    {
        try
        {
            string json = JsonSerializer.Serialize(payload, AppJsonContext.Context.SliderPayloadDto);
            string sign = Convert.ToBase64String(_security.AesCbcEncrypt(this.PrefixProcess(json), captcha.SmallImage.ToBytes()[^16..], _security.GenIV()));
            var content = new Dictionary<string, string> { {"sign", sign} };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.AUTHSERVER_CAPTCHA_VERIFY, content);
            using var response = await _client.SendAsync(request);
            var r = await response.Content.ReadAsStringAsync();
            return r.Contains("success");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("校验验证码失败 {Exception}", e);
            return false;
        }
    }
}
