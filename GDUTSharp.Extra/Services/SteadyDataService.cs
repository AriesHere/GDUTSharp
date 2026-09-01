using System.Net;
using GDUTSharp.Interfaces;
using GDUTSharp.Services;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Extra.Services;

/// <summary>
/// 更稳健的 DataService
/// </summary>
/// <remarks>
/// 重写了 <see cref="Login(LoginInfo)"/> 方法以避免由激进的优化导致的错误
/// </remarks>
public class SteadyDataService(ILogger<DataService> logger, ICommonClient client, ISecurityService security)
    : DataService(logger, client, security)
{
    public async override Task<bool> Login(LoginInfo loginInfo)
    {
        HttpResponseMessage? response = null;
        try
        {
            if (loginInfo.Role != Role.UNDER_GRADUATE)
            {
                throw new NotSupportedException("不支持除本科生以外身份的操作");
            }
            using var request = new HttpRequestMessage(HttpMethod.Get, GDUTConstant.AUTHSERVER_LOGIN_URL);
            var formData = new Dictionary<string, string>();

            using (var tempResponse = await _client.SendAsync(request))
            {
                string pwdEncryptSalt = string.Empty;
                string html = await tempResponse.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
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
                formData["password"] = _security.CbcEncrypt(loginInfo.Password, pwdEncryptSalt);
            }

            response = await this.Post(formData, GDUTConstant.AUTHSERVER_LOGIN_URL, GDUTConstant.AUTHSERVER_LOGIN_URL);

            for (int i = 0; i < 5; i++)
            {
                if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.MovedPermanently)
                    break;
                string? location = response.Headers.Location?.AbsoluteUri;
                if (string.IsNullOrEmpty(location))
                    break;
                if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", i, location);
                response.Dispose();
                using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                response = await _client.SendAsync(redirectRequest);
            }

            var result = response.StatusCode == HttpStatusCode.Found;
            _role = result ? loginInfo.Role : null;
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("认证异常。 {Exception}", e);
            return false;
        }
        finally
        {
            response?.Dispose();
        }
    }
}
