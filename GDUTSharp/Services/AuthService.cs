using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Attributes;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

[RegisterToDI(nameof(IAuthService))]
public class AuthService(ILogger<AuthService> logger, ICommonClient client, ISecurityService security) : IAuthService
{
    protected readonly ILogger<AuthService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly ISecurityService _security = security;
    protected const int PrefixLength = 64;
    protected const int IvLength = 16;
    /// <remarks>疑似只接受此字符集</remarks>
    protected const string AesChars = "ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz2345678";

    #region 加密

    protected static byte[] GenIV()
    {
        var bytes = new byte[IvLength];
        for (int i = 0; i < IvLength; i++)
        {
            int index = RandomNumberGenerator.GetInt32(AesChars.Length);
            bytes[i] = (byte)AesChars[index];
        }
        return bytes;
    }

    protected static byte[] GenPrefix()
    {
        var bytes = new byte[PrefixLength];
        for (int i = 0; i < PrefixLength; i++)
        {
            int index = RandomNumberGenerator.GetInt32(AesChars.Length);
            bytes[i] = (byte)AesChars[index];
        }
        return bytes;
    }

    /// <summary>附加前缀</summary>
    protected virtual byte[] PrefixProcess(string raw) => [..GenPrefix(), ..raw.ToBytes()];

    #endregion

    protected virtual string GenUrl(AuthServices service) => service switch
    {
        AuthServices.AcademicAffairs => GSConst.AuthServerAuth + GSConst.AcademicAffairsUnderLogin,
        AuthServices.Library => GSConst.AuthServerAuth + GSConst.LibraryLogin,
        AuthServices.Ehall => GSConst.AuthServerAuth + GSConst.EhallLogin,
        AuthServices.AuthServer or _ => GSConst.AuthServerLogin,
    };

    public virtual async Task<HttpResponseMessage?> LoginAndAuth(
        CookieContainer cookies,
        AuthServices service = AuthServices.AuthServer,
        LoginInfo? loginInfo = null,
        CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        try
        {
            var url = this.GenUrl(service);
            request = new HttpRequestMessage(HttpMethod.Post, url);
            HttpResponseMessage response = await _client.SendAsync(cookies, request, true, token);
            request.Dispose();

            if ((await response.Content.ReadAsStringAsync(token)).StartsWith("﻿﻿<!DOCTYPE html><html class=\"root-main\">"))   // 需要登录
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

                    var pwdEncryptSalt = html.Extract("id=\"pwdEncryptSalt\" value=\"", '"', out var cur);
                    var execution = html.Extract("name=\"execution\" value=\"", '"', out _, cur);
                    formData["username"] = loginInfo.UserName;
                    formData["password"] = Convert.ToBase64String(
                        _security.Encrypt(this.PrefixProcess(loginInfo.Password), pwdEncryptSalt.ToBytes(), GenIV()));
                    formData["captcha"] = "";
                    formData["_eventId"] = "submit";
                    formData["cllt"] = "userNameLogin";
                    formData["dllt"] = "generalLogin";
                    formData["lt"] = "";
                    formData["execution"] = execution;

                    request = ICommonClient.CreateRequest(HttpMethod.Post, url, formData);
                    response = await _client.SendAsync(cookies, request, true, token);
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
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AuthServerLogout);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, false, token);
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
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AuthServerCheckCaptcha + username);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, false, token);
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
            using HttpRequestMessage request = new(HttpMethod.Get, GSConst.AuthServerGetCaptcha);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, false, token);
            return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.AuthServerCaptcha, token);
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
        byte[] safeSecure,
        CancellationToken token = default)
    {
        try
        {
            string json = JsonSerializer.Serialize(payload, AppJsonContext.Context.SliderPayloadDto);
            string sign = Convert.ToBase64String(_security.Encrypt(this.PrefixProcess(json), safeSecure, RandomNumberGenerator.GetBytes(16)));
            var content = new Dictionary<string, string> { {"sign", sign} };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.AuthServerVerifyCaptcha, content);
            using var response = await _client.SendAsync(cookies, request, false, token);
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
