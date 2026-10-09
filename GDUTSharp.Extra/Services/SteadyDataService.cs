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
    public async override Task<HttpResponseMessage?> LoginAndAuth(
        CookieContainer cookies,
        IAuthService.SupportedServices? service = null,
        LoginInfo? loginInfo = null,
        CancellationToken token = default)
    {
        try
        {
            var url = service switch
            {
                IAuthService.SupportedServices.JXFW => GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.UNDER_GRADUATE_LOGIN,
                IAuthService.SupportedServices.LIBRARY => GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.LIBRARY_LOGIN,
                _ => GSConst.AUTHSERVER_LOGIN,
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            HttpResponseMessage response = await _client.SendAsync(cookies, request, token);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // 需要登录
                if (loginInfo is null)
                {
                    Log.LoginFailed(_logger, null);
                    return null;
                }
                else
                {
                    Log.LoginAndAuth(_logger);
                    var formData = new Dictionary<string, string>();
                    string pwdEncryptSalt = string.Empty;
                    var doc = new HtmlDocument();
                    doc.Load(await response.Content.ReadAsStreamAsync(token));
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

                    formData["username"] = loginInfo.UserName;
                    formData["password"] = Convert.ToBase64String(
                        _security.AesCbcEncrypt(base.PrefixProcess(loginInfo.Password), pwdEncryptSalt.ToBytes(), GenIV()));

                    using var request2 = ICommonClient.CreateRequest(
                        HttpMethod.Post,
                        GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.UNDER_GRADUATE_LOGIN,
                        formData,
                        GSConst.UNDER_GRADUATE_LOGIN);
                    response = await _client.SendAsync(cookies, request2, token);
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
    }
}
