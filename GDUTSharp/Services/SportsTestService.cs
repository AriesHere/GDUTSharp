using System.Security.Cryptography;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class SportsTestService(ILogger<SportsTestService> logger, ICommonClient client, ISecurityService security) : ISportsTestService
{
    protected ILogger<SportsTestService> _logger = logger;
    protected ICommonClient _client = client;
    protected ISecurityService _security = security;

    public async virtual Task<byte[]?> GetCaptcha()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.SPORTS_TEST_CAPTCHA);
            using var response = await _client.SendAsync(request);
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("获取验证码失败。{e}", e);
            return null;
        }
    }

    public async virtual Task<bool> Login(LoginInfo loginInfo)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            request = new(HttpMethod.Get, GSConst.SPORTS_TEST_BASE);
            response = await _client.SendAsync(request);
            request.Dispose();
            var content = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var newUrl = GSConst.SPORTS_TEST_BASE + content.Extract("window.location.href='", '\'', out _);
            request = new(HttpMethod.Get, newUrl);
            response = await _client.SendAsync(request);
            request.Dispose();
            content = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var viewState = content.Extract("id=\"__VIEWSTATE\" value=\"", '"', out var cur);
            var viewStateGenerator = content.Extract("id=\"__VIEWSTATEGENERATOR\" value=\"", '"', out cur, cur);
            var btnSubmit = content.Extract("name=\"BtnSubmit\" value=\"", '"', out cur, cur);
            var hidSchoolNo = content.Extract("id=\"hidSchoolNo\" value=\"", '"', out cur, cur);
            var hidRole = content.Extract("id=\"hidRole\" value=\"", '"', out cur, cur);
            var hidCode = content.Extract("id=\"hidCode\" value=\"", '"', out cur, cur);
            var hidCategory = content.Extract("id=\"hidCategory\" value=\"", '"', out cur, cur);
            var hidEduYears = content.Extract("id=\"hidEduYears\" value=\"", '"', out cur, cur);
            var hidCurrentSchoolYear = content.Extract("id=\"hidCurrentSchoolYear\" value=\"", '"', out cur, cur);
            var hidCurrentSchoolTerm = content.Extract("id=\"hidCurrentSchoolTerm\" value=\"", '"', out cur, cur);
            var hidAdvanceTest = content.Extract("id=\"hidAdvanceTest\" value=\"", '"', out cur, cur);
            var hidStudentLoginPwdMode = content.Extract("id=\"hidStudentLoginPwdMode\" value=\"", '"', out cur, cur);
            var hidStudentLoginPwdText = content.Extract("id=\"hidStudentLoginPwdText\" value=\"", '"', out cur, cur);
            var hidIsBridge = content.Extract("id=\"hidIsBridge\" value=\"", '"', out cur, cur);
            var hidModelWeixin = content.Extract("id=\"hidModelWeixin\" value=\"", '"', out cur, cur);
            var hidReferUrl = content.Extract("id=\"hidReferUrl\" value=\"", '"', out cur, cur);

            byte[] key = SHA256.HashData(hidSchoolNo.ToBytes())[0..16];
            byte[] iv = hidCode.ToBytes()[..ISecurityService.IV_LEN];
            var requestContent = new Dictionary<string, string>
            {
                // 疑似缺一不可
                { "__VIEWSTATE", viewState },
                { "__VIEWSTATEGENERATOR", viewStateGenerator },
                { "username", loginInfo.UserName },
                { "password", Convert.ToBase64String(_security.AesCbcEncrypt(loginInfo.Password.ToBytes(), key, iv)) },
                { "vercode", loginInfo.Captcha },
                { "drpRole", "2" },
                { "BtnSubmit", btnSubmit },
                { "hidSchoolNo", hidSchoolNo },
                { "hidRole", hidRole },
                { "hidCode", hidCode },
                { "hidMac", "" },
                { "hidCategory", hidCategory },
                { "hidEduYears", hidEduYears },
                { "hidCurrentSchoolYear", hidCurrentSchoolYear },
                { "hidCurrentSchoolTerm", hidCurrentSchoolTerm },
                { "hidAdvanceTest", hidAdvanceTest },
                { "hidStudentLoginPwdMode", hidStudentLoginPwdMode },
                { "hidStudentLoginPwdText", hidStudentLoginPwdText },
                { "hidIsBridge", hidIsBridge },
                { "hidModelWeixin", hidModelWeixin },
                { "hidReferUrl", hidReferUrl },
            };
            request = ICommonClient.CreateRequest(HttpMethod.Post, newUrl, requestContent);
            response = await _client.SendAsync(request);
            request.Dispose();
            var r = await response.Content.ReadAsStringAsync();
            return r.StartsWith("<script language='javascript'>");
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("登录失败。{e}", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }
}
