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

    public async virtual Task<bool> Auth(IAuthService.SupportedServices services)
    {
        HttpResponseMessage? response = null;
        try
        {
            var url = services switch
            {
                IAuthService.SupportedServices.JXFW => GDUTConstant.UNDER_GRADUATE_LOGIN,
                IAuthService.SupportedServices.LIBRARY => GDUTConstant.LIBRARY_LOGIN,
                _ => GDUTConstant.UNDER_GRADUATE_LOGIN,
            };
            if (!url.StartsWith(GDUTConstant.AUTHSERVER_AUTH_Prefix))
            {
                url = GDUTConstant.AUTHSERVER_AUTH_Prefix + url;
            }
            using var postRequest = new HttpRequestMessage(HttpMethod.Post, url);
            response = await _client.SendAsync(postRequest);
            _logger.LogError("状态码：\n{Content}", response.StatusCode);
            _logger.LogError("内容：\n{Content}", response.Content.ReadAsStringAsync().Result.Length);

            for (int i = 0; i < 10; i++)
            {
                if (response.StatusCode != HttpStatusCode.Redirect)
                    break;
                string? location = response.Headers.Location?.AbsoluteUri;
                if (string.IsNullOrEmpty(location))
                    break;
                if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", i + 1, location);
                response.Dispose();
                using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                response = await _client.SendAsync(redirectRequest);
            }

            // TODO: 这只是权宜之计
            if (url == GDUTConstant.AUTHSERVER_AUTH_Prefix + GDUTConstant.LIBRARY_LOGIN)
            {
                var r = await response.Content.ReadAsStringAsync();
                int valueIndex = r.IndexOf("value=\"");
                int start = valueIndex + "value=\"".Length;
                int end = r.IndexOf('"', start);
                string refValue = WebUtility.HtmlDecode(r[start..end]);
                response.Dispose();
                var rq = new HttpRequestMessage(HttpMethod.Get, refValue);
                rq.Headers.Referrer = new(GDUTConstant.LIBRARY_LOGIN);
                response = await _client.SendAsync(rq);

                var r1 = await response.Content.ReadAsStringAsync();
                int valueIndex1 = r1.IndexOf("name=\"refer\" value=\"");
                int start1 = valueIndex1 + "name=\"refer\" value=\"".Length;
                int end1 = r1.IndexOf('"', start1);
                string refValue1 = WebUtility.HtmlDecode(r1[start1..end1]);
                response.Dispose();
                var rq1 = new HttpRequestMessage(HttpMethod.Get, refValue1);
                response = await _client.SendAsync(rq1);

                Console.WriteLine(response.Headers.Location);
                // 这里的 Location 形式如下 https://opac.gdut.edu.cn/#/Home?jwt=XXXXXXX&jwtHeader=jwtOpacAuth&pageType=0&navigation=1
                // 后面如果想要正常取得数据，需要在请求头中加入 jwtOpacAuth: XXXXXXX
                // 累了，需要大改
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                // 摘要:
                // ?< !DOCTYPE html >< html class="root-main">
                // <!-- 移动端 --><!-- PC端 --><!--校外用户登录--><!-- 二维码扫码登录 --><!-- 兼容性登录 --><!--校外用户登录--><!-- 帐号登录或动态码登录 --><head>
                //         <meta charset = "utf-8" />
                using var reader = new StreamReader(response.Content.ReadAsStream());
                reader.ReadLine();  // skip
                return reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == false;
            }
            else return false;
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

    public async virtual Task<bool> Login(LoginInfo loginInfo)
    {
        HttpResponseMessage? response = null;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GDUTConstant.AUTHSERVER_LOGIN_URL);
            var formData = new Dictionary<string, string>();

            using (var tempResponse = await _client.SendAsync(request))
            {
                string pwdEncryptSalt = string.Empty;
                string html = await tempResponse.Content.ReadAsStringAsync();

                // 这里采用了相当激进的优化，如果校方改东西了，可能会出错。如果不希望这
                // 样，请使用 GDUTSharp.Extra.SteadyDataService 中的 Login 方法
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
            }

            using var request2 = ICommonClient.CreateRequest(
                HttpMethod.Post,
                GDUTConstant.AUTHSERVER_AUTH_Prefix + GDUTConstant.UNDER_GRADUATE_LOGIN,
                formData,
                GDUTConstant.UNDER_GRADUATE_LOGIN);
            response = await _client.SendAsync(request2);

            for (int i = 0; i < 10; i++)
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

            var result = response.StatusCode == HttpStatusCode.OK;
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

    public async virtual Task<bool> Logout()
    {
        try
        {
            using HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, GDUTConstant.AUTHSERVER_LOGOUT_URL));
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
}
