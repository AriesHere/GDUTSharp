using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using GDUTSharp.Interfaces;
using GDUTSharp.Json;
using GDUTSharp.Type;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services
{
    /// <remarks>
    /// <para>请勿将其注册为单例</para>
    /// <para>碎碎念:</para>
    /// <para>据测试，正常情况下<see cref="Login(LoginInfo)"/> 会重定向五次，<see cref="Auth(string)"/>会重定向三次（虽然不知道这个数据有什么用</para>
    /// </remarks>
    public class DataService(ILogger<DataService> logger, ICommonClient client, ISecurityService security) : IDataService
    {
        private readonly ILogger<DataService> _logger = logger;
		private readonly ICommonClient _client = client;
        private readonly ISecurityService _security = security;
        private Role? _role = null;

        public bool Login(LoginInfo loginInfo)
        {
            HttpResponseMessage? response = null;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, Constant.AUTHSERVER_LOGIN_URL);
                string html = _client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var formData = new Dictionary<string, string>();
                string pwdEncryptSalt = string.Empty;
                var hiddenInputs = doc.DocumentNode.SelectNodes("//*[@id=\"pwdFromId\"]//input[@type=\"hidden\"]"); // 对应原来的css选择器 #pwdFromId input[type=hidden]
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

                using var postRequest = new HttpRequestMessage(HttpMethod.Post, Constant.AUTHSERVER_LOGIN_URL)
                {
                    Content = new FormUrlEncodedContent(formData)
                };
                response = _client.SendAsync(postRequest).Result;

                int maxRedirects = 5;
                int redirectCount = 0;
                bool isRedirect = response.StatusCode == HttpStatusCode.MovedPermanently
                    || response.StatusCode == HttpStatusCode.Redirect
                    || response.StatusCode == HttpStatusCode.SeeOther
                    || response.StatusCode == HttpStatusCode.TemporaryRedirect
                    || response.StatusCode == HttpStatusCode.PermanentRedirect;
                while (isRedirect && redirectCount < maxRedirects)
                {
                    string? location = response.Headers.Location?.AbsoluteUri;
                    if (string.IsNullOrEmpty(location))
                        break;

                    redirectCount++;
                    if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", redirectCount, location);
                    response.Dispose();
                    var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                    response = _client.SendAsync(redirectRequest).Result;
                }
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    _role = loginInfo.Role;
                    return true;
                }
                else
                {
                    return false;
                }
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

        public bool Auth(string url)
        {
            HttpResponseMessage? response = null;
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                if (!url.StartsWith(Constant.AUTHSERVER_AUTH_Prefix))
                {
                    url = Constant.AUTHSERVER_AUTH_Prefix + url;
                }
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                string html = _client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

                using var postRequest = new HttpRequestMessage(HttpMethod.Post, url);
                response = _client.SendAsync(postRequest).Result;

                int maxRedirects = 5;
                int redirectCount = 0;
                bool isRedirect = response.StatusCode == HttpStatusCode.MovedPermanently
                    || response.StatusCode == HttpStatusCode.Redirect
                    || response.StatusCode == HttpStatusCode.SeeOther
                    || response.StatusCode == HttpStatusCode.TemporaryRedirect
                    || response.StatusCode == HttpStatusCode.PermanentRedirect;
                while (isRedirect && redirectCount < maxRedirects)
                {
                    string? location = response.Headers.Location?.AbsoluteUri;
                    if (string.IsNullOrEmpty(location))
                        break;

                    redirectCount++;
                    if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", redirectCount, location);
                    response.Dispose();
                    var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                    response = _client.SendAsync(redirectRequest).Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    // 摘要:
                    // ?< !DOCTYPE html >< html class="root-main">
                    // <!-- 移动端 --><!-- PC端 --><!--校外用户登录--><!-- 二维码扫码登录 --><!-- 兼容性登录 --><!--校外用户登录--><!-- 帐号登录或动态码登录 --><head>
                    //         <meta charset = "utf-8" />
                    Stream s = response.Content.ReadAsStream();
                    TextReader reader = new StreamReader(s);
                    reader.ReadLine();  // skip
                    return reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == false;
                }
                else
                {
                    return false;
                }
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

        public string? GetTerm()
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                using var request = new HttpRequestMessage(HttpMethod.Post, Constant.UNDER_CLAZZ_TERM);
                request.Headers.Referrer = new(Constant.UNDER_REFER);
                string content = _client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                int index = content.IndexOf("selected");
                return content[(index - 2 - "202502".Length)..(index - 2)];
                // content 摘要:
                // <option value='202601' >2026秋季</option><option value='202502' selected>2026春季</option><option value='202501' >2025秋季</option>
                // 可见: 20XX春季 => 20XX01, 20XX秋季 => 20XY02 (Y == X + 1)
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求学期异常。 {Exception}", e);
                return null;
            }
        }

        public LessonCollection? GetLessons(string term)
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                var temp = new Dictionary<string, string>
                {
                    { "xnxqdm", term },
                    { "zc", "" },
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "zc,xq,jcdm" },
                    { "order", "asc" },
                };
                using var content = new FormUrlEncodedContent(temp);
                using var request = new HttpRequestMessage(HttpMethod.Post, Constant.UNDER_CLAZZ)
                {
                    Content = content
                };
                request.Headers.Referrer = new(Constant.UNDER_REFER);
                using HttpResponseMessage response = _client.SendAsync(request).Result;
                var result = response.Content.ReadFromJsonAsync(AppJsonContext.Context().LessonCollection).Result;
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课表异常。 {Exception}", e);
                return null;
            }
        }

        public ExamCollection? GetExamSchedule(string term)
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                var temp = new Dictionary<string, string>
                    {
                        { "xnxqdm", term },
                        { "page", "1" },
                        { "rows", "200" },
                        { "sort", "zc,xq,jcdm2" },
                        { "order", "asc" },
                    };
                using var content = new FormUrlEncodedContent(temp);
                using var request = new HttpRequestMessage(HttpMethod.Post, Constant.UNDER_EXAM)
                {
                    Content = content
                };
                request.Headers.Referrer = new(Constant.UNDER_REFER);
                using HttpResponseMessage response = _client.SendAsync(request).Result;
                var result = response.Content.ReadFromJsonAsync(AppJsonContext.Context().ExamCollection).Result;
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试安排异常。 {Exception}", e);
                return null;
            }
        }

        public LessonScoreCollection? GetScore(string term)
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                // TODO: 处理 term == "" 时劳动教育成绩不显示的问题
                var temp = new Dictionary<string, string>
                    {
                        { "xnxqdm", term },
                        { "jhlxdm", "" },
                        { "page", "1" },
                        { "rows", "200" },
                        { "sort", "xnxqdm" },
                        { "order", "asc" },
                    };
                using var content = new FormUrlEncodedContent(temp);
                using var request = new HttpRequestMessage(HttpMethod.Post, Constant.UNDER_EXAME_SCORE)
                {
                    Content = content
                };
                request.Headers.Referrer = new(Constant.UNDER_REFER);
                using HttpResponseMessage response = _client.SendAsync(request).Result;
                var result = response.Content.ReadFromJsonAsync(AppJsonContext.Context().LessonScoreCollection).Result;
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试成绩异常。 {Exception}", e);
                return null;
            }
        }

        public CourseSelCollection? GetCourseSelection()
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                var temp = new Dictionary<string, string>
                    {
                        { "page", "1" },
                        { "rows", "200" },
                        { "sort", "xnxqdm" },
                        { "order", "asc" },
                    };
                using var content = new FormUrlEncodedContent(temp);
                using var request = new HttpRequestMessage(HttpMethod.Post, Constant.UNDER_COURSE_SEL)
                {
                    Content = content
                };
                request.Headers.Referrer = new(Constant.UNDER_REFER);
                using HttpResponseMessage response = _client.SendAsync(request).Result;
                var result = response.Content.ReadFromJsonAsync(AppJsonContext.Context().CourseSelCollection).Result;
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试成绩异常。 {Exception}", e);
                return null;
            }
        }
    }
}
