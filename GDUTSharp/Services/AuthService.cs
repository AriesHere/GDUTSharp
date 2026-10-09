using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
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
    protected const int PREFIX_LEN = 64;
    protected const string AES_CHARS = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";
    protected const string GET_SALT = "id=\"pwdEncryptSalt\" value=\"";
    protected const string GET_EXEC = "name=\"execution\" value=\"";

    #region 加密

    protected static byte[] GenIV()
    {
        var bytes = new byte[ISecurityService.IV_LEN];
        for (int i = 0; i < ISecurityService.IV_LEN; i++)
        {
            int index = RandomNumberGenerator.GetInt32(AES_CHARS.Length);
            bytes[i] = (byte)AES_CHARS[index];
        }
        return bytes;
    }

    protected static byte[] GenPrefix()
    {
        var bytes = new byte[PREFIX_LEN];
        for (int i = 0; i < PREFIX_LEN; i++)
        {
            int index = RandomNumberGenerator.GetInt32(AES_CHARS.Length);
            bytes[i] = (byte)AES_CHARS[index];
        }
        return bytes;
    }

    /// <summary>附加前缀</summary>
    protected virtual byte[] PrefixProcess(string raw) => [..GenPrefix(), ..raw.ToBytes()];

    #endregion

    public virtual async Task<HttpResponseMessage?> LoginAndAuth(
        CookieContainer cookies,
        IAuthService.SupportedServices? service = null,
        LoginInfo ? loginInfo = null,
        CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        try
        {
            var url = service switch
            {
                IAuthService.SupportedServices.JXFW => GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.UNDER_GRADUATE_LOGIN,
                IAuthService.SupportedServices.LIBRARY => GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.LIBRARY_LOGIN,
                _ => GSConst.AUTHSERVER_LOGIN,
            };
            request = new HttpRequestMessage(HttpMethod.Post, url);
            HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            request.Dispose();

            if (response.StatusCode == HttpStatusCode.OK)   // 需要登录
            {
                if (loginInfo is null)
                {
                    Log.LoginFailed(_logger, null);
                    return null;
                }
                else
                {
                    Log.LoginAndAuth(_logger);
                    var formData = new Dictionary<string, string>();
                    string html = await response.Content.ReadAsStringAsync(token);
                    response.Dispose();

                    // 这里采用了相当激进的优化，如果校方改东西了，可能会出错。如果不希望这样，
                    // 请使用 GDUTSharp.Extra.SteadyAuthService 中的 LoginAndAuth 方法
                    var pwdEncryptSalt = html.Extract(GET_SALT, '"', out var cur);
                    var execution = html.Extract(GET_EXEC, '"', out _, cur);
                    formData["username"] = loginInfo.UserName;
                    formData["password"] = Convert.ToBase64String(
                        _security.AesCbcEncrypt(this.PrefixProcess(loginInfo.Password), pwdEncryptSalt.ToBytes(), GenIV()));
                    formData["captcha"] = "";   // TODO
                    formData["_eventId"] = "submit";
                    formData["cllt"] = "userNameLogin";
                    formData["dllt"] = "generalLogin";
                    formData["lt"] = "";
                    formData["execution"] = execution;

                    request = ICommonClient.CreateRequest(
                        HttpMethod.Post,
                        GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.UNDER_GRADUATE_LOGIN,
                        formData,
                        GSConst.UNDER_GRADUATE_LOGIN);
                    response = await _client.SendAsync(cookies, request, token);
                }
            }
            else Log.Login(_logger);

            return response;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "登录或认证", e);
            return null;
        }
        finally
        {
            request?.Dispose();
        }
    }

    public virtual async Task<bool> Logout(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AUTHSERVER_LOGOUT);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var r = await response.Content.ReadAsStringAsync(token);
            return r.Contains("注销成功");
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "退出登录", e);
            return false;
        }
    }

    public virtual async Task<bool> CheckNeedCaptcha(CookieContainer cookies, string username, CancellationToken token = default)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AUTHSERVER_CHECK_CAPTCHA_PREFIX + username);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var r = await response.Content.ReadAsStringAsync(token);
            return r.Contains("true");
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "检查是否需要验证码", e);
            return false;
        }
    }

    public virtual async Task<AuthServerCaptcha?> GetCaptcha(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AUTHSERVER_CAPTCHA_GET);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var r = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.AuthServerCaptchaDto, token);
            if (r is null) return null;
            return r;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "获取验证码", e);
            return null;
        }
    }

    public virtual async Task<bool> SubmitCaptcha(
        CookieContainer cookies,
        SliderPayloadDto payload,
        AuthServerCaptcha captcha,
        CancellationToken token = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(payload, AppJsonContext.Context.SliderPayloadDto);
            string sign = Convert.ToBase64String(_security.AesCbcEncrypt(this.PrefixProcess(json), captcha.SmallImage.ToBytes()[^16..], _security.GenIV()));
            var content = new Dictionary<string, string> { {"sign", sign} };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.AUTHSERVER_CAPTCHA_VERIFY, content);
            using var response = await _client.SendAsync(cookies, request, token);
            var r = await response.Content.ReadAsStringAsync(token);
            return r.Contains("success");
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "校验验证码", e);
            return false;
        }
    }
}
