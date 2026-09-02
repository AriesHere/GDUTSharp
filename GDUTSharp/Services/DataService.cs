using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;
using static GDUTSharp.Interfaces.IDataService;

namespace GDUTSharp.Services
{
    /// <remarks>
    /// <para>请勿将其注册为单例</para>
    /// <para>碎碎念:</para>
    /// <para>据测试，正常情况下<see cref="Login(LoginInfo)"/> 会重定向五次，<see cref="Auth(string)"/>会重定向三次（虽然不知道这个数据有什么用</para>
    /// </remarks>
    public partial class DataService(ILogger<DataService> logger, ICommonClient client, ISecurityService security) : IDataService
    {
        protected readonly ILogger<DataService> _logger = logger;
        protected readonly ICommonClient _client = client;
        protected readonly ISecurityService _security = security;
        protected Role? _role = null;

        protected async Task<HttpResponseMessage> Post(Dictionary<string, string> content, string url, string referer)
        {
            using var c = new FormUrlEncodedContent(content);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = c
            };
            request.Headers.Referrer = new(referer);
            return await _client.SendAsync(request);
        }

        public async virtual Task<bool> Login(LoginInfo loginInfo)
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

                    // 这里采用了相当激进的优化，如果校方改东西了，可能会出错。如果不希望这
                    // 样，请使用 GDUTSharp.Extra.SteadyDataService 中的 Login 方法
                    Match saltMatch = Login_SaltRegex().Match(html);
                    pwdEncryptSalt = saltMatch.Success ? saltMatch.Groups[1].Value : "";
                    Match execMatch = Login_ExecRegex().Match(html);
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

        [GeneratedRegex(@"id=""pwdEncryptSalt""[^>]*?value=""([^""]*)""")]
        protected static partial Regex Login_SaltRegex();

        [GeneratedRegex(@"id=""execution""[^>]*?value=""([^""]*)""")]
        protected static partial Regex Login_ExecRegex();

        public async virtual Task<bool> Auth(SupportedServices service)
        {
            return await Auth(
                service switch 
                { 
                    SupportedServices.JXFW => GDUTConstant.UNDER_GRADUATE_LOGIN,
                    _ => GDUTConstant.UNDER_GRADUATE_LOGIN,
                }
            );
        }

        public async virtual Task<bool> Auth(string url)
        {
            HttpResponseMessage? response = null;
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                if (!url.StartsWith(GDUTConstant.AUTHSERVER_AUTH_Prefix))
                {
                    url = GDUTConstant.AUTHSERVER_AUTH_Prefix + url;
                }
                using var postRequest = new HttpRequestMessage(HttpMethod.Post, url);
                response = await _client.SendAsync(postRequest);

                for (int i = 0; i < 5; i++)
                {
                    if (response.StatusCode != HttpStatusCode.Redirect)
                        break;
                    string? location = response.Headers.Location?.AbsoluteUri;
                    if (string.IsNullOrEmpty(location))
                        break;
                    if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug("[第 {redirectCount} 次重定向] → {location}", i, location);
                    response.Dispose();
                    using var redirectRequest = new HttpRequestMessage(HttpMethod.Get, location);
                    response = await _client.SendAsync(redirectRequest);
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

        public async virtual Task<string?> GetTerm()
        {
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                using var request = new HttpRequestMessage(HttpMethod.Post, GDUTConstant.UNDER_CLAZZ_TERM);
                request.Headers.Referrer = new(GDUTConstant.UNDER_CLAZZ_TERM);
                using var response = await _client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();
                int index = content.IndexOf("selected");
                return content[(index - 2 - "202502".Length)..(index - 2)];
                // content 摘要:
                // <option value='202601' >2026秋季</option><option value='202502' selected>2026春季</option><option value='202501' >2025秋季</option>
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求学期异常。 {Exception}", e);
                return null;
            }
        }

        public async virtual Task<List<Lesson>?> GetLessons(string term)
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
                using HttpResponseMessage response = await this.Post(temp, GDUTConstant.UNDER_CLAZZ, GDUTConstant.UNDER_CLAZZ);
                return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.LessonDtoCollection);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课表异常。 {Exception}", e);
                return null;
            }
        }

        public async virtual Task<List<ExamSchedule>?> GetExamSchedule(string term)
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
                    { "rows", "300" },
                    { "sort", "zc,xq,jcdm2" },
                    { "order", "asc" },
                };
                using HttpResponseMessage response = await this.Post(temp, GDUTConstant.UNDER_EXAM, GDUTConstant.UNDER_EXAM);
                return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ExamScheduleDtoCollection);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试安排异常。 {Exception}", e);
                return null;
            }
        }

        public async virtual Task<List<CourseScore>?> GetCourseScore(string term)
        {
            HttpResponseMessage? response = null;
            try
            {
                if (_role != Role.UNDER_GRADUATE)
                {
                    throw new NotSupportedException("不支持除本科生以外身份的操作");
                }
                var temp = new Dictionary<string, string>
                {
                    { "xnxqdm", term },
                    { "jhlxdm", "" },
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "xnxqdm" },
                    { "order", "asc" },
                };
                response = await this.Post(temp, GDUTConstant.UNDER_EXAM_SCORE, GDUTConstant.UNDER_EXAM_SCORE);
                var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.CourseScoreDtoCollection);
                response.Dispose();
                if (result != null && term == "")
                {
                    HashSet<string> terms = [];
                    foreach (var item in result.rows)
                        terms.Add(item.xnxqmc);
                    foreach (var item in terms)
                    {
                        temp["xnxqdm"] = Helper.TermStringToInt6Digit(item).ToString();
                        response = await this.Post(temp, GDUTConstant.UNDER_EXAM_SCORE, GDUTConstant.UNDER_EXAM_SCORE);
                        var tempResult = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.CourseScoreDtoCollection);
                        response.Dispose();
                        if (tempResult != null)
                        {
                            foreach (var scoreItem in tempResult.rows)
                            {
                                if (scoreItem.kcmc == "劳动教育")
                                {
                                    result.Add(scoreItem);
                                    goto END;
                                }
                            }
                        }
                    }
                };
                END:
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试成绩异常。 {Exception}", e);
                return null;
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async virtual Task<List<CourseSel>?> GetCourseSelection()
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
                    { "rows", "300" },
                    { "sort", "kcflmc" },
                };
                using HttpResponseMessage response = await this.Post(temp, GDUTConstant.UNDER_COURSE_SEL, GDUTConstant.UNDER_COURSE_SEL);
                return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.CourseSelDtoCollection);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求可选课列表异常。 {Exception}", e);
                return null;
            }
        }

        public async virtual Task<List<CourseSel>?> GetSelectedCourse()
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
                    { "rows", "300" },
                    { "sort", "kcflmc" },
                };
                using HttpResponseMessage response = await this.Post(temp, GDUTConstant.UNDER_COURSE_SEL_ED, GDUTConstant.UNDER_COURSE_SEL_ED);
                var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ListCourseSelDto);
                if (result is null) return null;
                else return [..result];
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求已选课列表异常。 {Exception}", e);
                return null;
            }
        }

        public async virtual Task<List<Lesson>?> GetCourseTask(string code)
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
                    { "rows", "300" },
                    { "kcrwdm", code },
                    { "sort", "zc,xq,jcdm" },
                    { "order", "asc" },
                };
                using HttpResponseMessage response = await this.Post(temp, GDUTConstant.UNDER_COURSE_TASK, GDUTConstant.UNDER_COURSE_TASK);
                var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ListLesson);
                return result;
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课程任务异常。 {Exception}", e);
                return null;
            }
        }
    }
}
