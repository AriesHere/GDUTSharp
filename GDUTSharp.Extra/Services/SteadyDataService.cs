using System.Net;
using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Extra.Services;

/// <summary>
/// 基于 HtmlAgilityPack 实现的更稳健的 AuthService
/// </summary>
/// <remarks>
/// 重写了 <see cref="LoginAndAuth(IAuthService.SupportedServices?, LoginInfo?)"/> 方法以避免由激进的优化导致的错误
/// </remarks>
public class SteadyAuthService(ILogger<SteadyAuthService> logger, ICommonClient client, ISecurityService security)
    : AuthService(logger, client, security)
{
    public async override Task<HttpResponseMessage?> LoginAndAuth(IAuthService.SupportedServices? service = null, LoginInfo? loginInfo = null)
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
                    var doc = new HtmlDocument();
                    doc.Load(await response.Content.ReadAsStreamAsync());
                    response.Dispose();
                    var hiddenInputs = doc.DocumentNode.SelectNodes("//*[@id=\"pwdFromId\"]//input[@type=\"hidden\"]") // 对应原来的css选择器 #pwdFromId input[type=hidden]
                        ?? throw new NullReferenceException("查找 html 元素失败");
                    foreach (var input in hiddenInputs)
                    {
                        string name = input.GetAttributeValue("name", "");
                        string value = input.GetAttributeValue("value", "");
                        string id = input.GetAttributeValue("id", "");

                        if (!string.IsNullOrEmpty(name))
                            formData[name] = value;

                        if (id == "pwdEncryptSalt")
                            pwdEncryptSalt = value;
                    }

                    formData[""] = pwdEncryptSalt;
                    formData["username"] = loginInfo.UserName;
                    formData["password"] = Convert.ToBase64String(
                        _security.AesCbcEncrypt(base.PrefixProcess(loginInfo.Password), pwdEncryptSalt.ToBytes(), INIT_VECTOR));

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
}
