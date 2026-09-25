using System.Reflection.Metadata.Ecma335;
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

    public async virtual Task<SportsTestScore?> GetScore(string year)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.SPORTS_TEST_SCORE + year);
            using var response = await _client.SendAsync(request);
            var html = await response.Content.ReadAsStringAsync();
            var cur = html.IndexOf("未查询到该学生ID");
            if (cur != -1) return null;   // 检查是否有成绩
            cur = 0;
            SportsTestScore result = new()
            {
                StudentName = html.Extract("<span id=\"lblStudentName\">", '<', out cur, cur),
                StudentId = html.Extract("<span id=\"lblStudentNO\">", '<', out cur, cur),
                Year = html.Extract("<br/>", '年', out cur, cur)    // 虽然输入了一个 year，但是以防万一，以返回结果为准
            };
            // 身高体重
            var tempIndex = html.IndexOf("lblHeight", cur);
            if (tempIndex != -1 && float.TryParse(html.Extract(">", '<', out cur, tempIndex), out var heightR))
                result.Height_R = heightR;
            (result.Figure_RS, result.Figure_WS) = SetScore(html, "lblBMI", ref cur);
            tempIndex = html.IndexOf("lblWeight", cur);
            if (tempIndex != -1 && float.TryParse(html.Extract(">", '<', out cur, tempIndex), out var weightR))
                result.Weight_R = weightR;
            // 肺活量
            tempIndex = html.IndexOf("lblVitalCapacity", cur);
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var lungR))
                result.VitalCapacity_R = lungR;
            (result.VitalCapacity_RS, result.VitalCapacity_WS) = SetScore(html, "lblVitalCapacity", ref cur);
            // 50米跑
            tempIndex = html.IndexOf("lblFiftyMeterRace", cur);
            if (tempIndex != -1)
            {
                var r = html.Extract(">", '<', out cur, tempIndex);
                if (r.Length > 0)
                {
                    var t = r.Split('.');
                    result.Meter50_R = t.Length == 2 ? new(0, 0, int.Parse(t[0]), int.Parse(t[1])) : new(0, 0, int.Parse(t[0]));
                }
            }
            (result.Meter50_RS, result.Meter50_WS) = SetScore(html, "lblFiftyMeterRace", ref cur);
            // 坐位体前屈
            tempIndex = html.IndexOf("lblSitAndReach", cur);
            if (tempIndex != -1 && float.TryParse(html.Extract(">", '<', out cur, tempIndex), out var sitR))
                result.SitAndReach_R = sitR;
            (result.SitAndReach_RS, result.SitAndReach_WS) = SetScore(html, "lblSitAndReach", ref cur);
            // 立定跳远
            tempIndex = html.IndexOf("lblLongJump", cur);
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var jumpR))
                result.StandingLongJump_R = jumpR;
            (result.StandingLongJump_RS, result.StandingLongJump_WS) = SetScore(html, "lblLongJump", ref cur);
            // 跳绳
            tempIndex = html.IndexOf("lblRopeSkipping", cur);
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var ropeR))
                result.RopeSkipping_R = ropeR;
            (result.RopeSkipping_RS, result.RopeSkipping_WS) = SetScore(html, "lblRopeSkipping", ref cur);
            // 引体向上
            tempIndex = html.IndexOf("lblChinning", cur);
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var pullR))
                result.PullUp_R = pullR;
            (result.PullUp_RS, result.PullUp_WS) = SetScore(html, "lblRopeSkipping", ref cur);
            // 仰卧起坐
            tempIndex = html.IndexOf("lblSitUps", cur);
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var upR))
                result.SitUp_R = upR;
            (result.SitUp_RS, result.SitUp_WS) = SetScore(html, "lblRopeSkipping", ref cur);
            // 千米跑
            tempIndex = html.IndexOf("lblKiloMeterRace", cur);
            if (tempIndex != -1)
            {
                var r = html.Extract(">", '<', out cur, tempIndex);
                if (r.Length > 0)
                {
                    var t = r.Split('.');
                    result.Meter1k_R = t.Length == 2 ? new(0, int.Parse(t[0]), int.Parse(t[1])) : new(0, int.Parse(t[0]));
                }
            }
            (result.Meter1k_RS, result.Meter1k_WS) = SetScore(html, "lblRopeSkipping", ref cur);
            // 800 米跑
            tempIndex = html.IndexOf("lblEightHundredMeterRace", cur);
            if (tempIndex != -1)
            {
                var r = html.Extract(">", '<', out cur, tempIndex);
                if (r.Length > 0)
                {
                    var t = r.Split('.');
                    result.Meter800_R = t.Length == 2 ? new(0, int.Parse(t[0]), int.Parse(t[1])) : new(0, int.Parse(t[0]));
                }
            }
            (result.Meter800_RS, result.Meter800_WS) = SetScore(html, "lblEightHundredMeterRace", ref cur);
            // 50米×8往返跑
            tempIndex = html.IndexOf("lblShuttleRun", cur);
            if (tempIndex != -1)
            {
                var r = html.Extract(">", '<', out cur, tempIndex);
                if (r.Length > 0)
                {
                    var t = r.Split('.');
                    result.RoundTrip_R = t.Length == 2 ? new(0, int.Parse(t[0]), int.Parse(t[1])) : new(0, int.Parse(t[0]));
                }
            }
            (result.RoundTrip_RS, result.RoundTrip_WS) = SetScore(html, "lblShuttleRun", ref cur);
            // 加权总分
            tempIndex = html.IndexOf("lblTotalScore", cur);
            if (tempIndex != -1 && float.TryParse(html.Extract(">", '<', out cur, tempIndex), out var ws))
                result.Score = ws;
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("获取体测成绩失败。{e}", e);
            return null;
        }

        static (int Raw, float Weighted) SetScore(string html, string find, ref int cur)
        {
            var tempIndex = html.IndexOf(find + "_Score", cur);
            (int Raw, float Weighted) result = new();
            if (tempIndex != -1 && int.TryParse(html.Extract(">", '<', out cur, tempIndex), out var rs))
                result.Raw = rs;
            tempIndex = html.IndexOf(find + "_SingleScore", cur);
            if (tempIndex != -1 && float.TryParse(html.Extract(">", '<', out cur, tempIndex), out var ws))
                result.Weighted = ws;
            return result;
        }
    }
}
