using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using GDUTSharp.Shared.Type.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GDUTSharp.Interfaces.IJXFWService;

namespace GDUTSharp.Services;

public class JXFWServiceOptions
{
    /// <summary>每次请求时请求的数据条目数</summary>
    public int ItemPerRequest { get; set; } = 300;

    /// <summary>请求时最大请求页数</summary>
    public int MaxPage { get; set; } = 10;
}

public class JXFWService(
    IOptions<JXFWServiceOptions> options,
    ILogger<JXFWService> logger,
    ICommonClient client,
    IAuthService authService,
    ISecurityService security)
    : IJXFWService
{
    protected readonly ILogger<JXFWService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly IAuthService _authService = authService;
    protected readonly ISecurityService _security = security;
    protected readonly int _maxPage = options.Value.MaxPage;
    protected readonly int _itemPerRequest = options.Value.ItemPerRequest;

    protected virtual async Task<List<TResult>?> GetData<TResult, TDto, TDtoCollection>(
        CookieContainer cookies,
        string url,
        Dictionary<string, string> requestContent,
        JsonTypeInfo<TDtoCollection> jsonTypeInfo,
        CancellationToken token = default)
        where TDtoCollection : DtoCollectionBase<TResult, TDto>
    {
        List<TResult>? r = null;
        requestContent.AddIfNotExist("rows", $"{_itemPerRequest}").AddIfNotExist("page", "1").AddIfNotExist("order", "asc");
        for (int i = 1; i <= _maxPage; i++)
        {
            requestContent["page"] = $"{i}";
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, url, requestContent, url);
            using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
            var temp = await response.Content.ReadFromJsonAsync(jsonTypeInfo, token);
            var tempResult = temp?.Convert();
            if (r is null)
            {
                r = tempResult;
            }
            else if (tempResult is not null)
            {
                r.AddRange(tempResult);
            }
            if (tempResult is null || tempResult.Count < _itemPerRequest)
            {
                break;
            }
        }
        return r;
    }

    public virtual async Task<byte[]?> GetCaptcha(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GSConst.UNDER_CAPTCHA + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            using var response = await _client.SendAsync(cookies, request, token);
            return await response.Content.ReadAsByteArrayAsync(token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求验证码", e);
            return null;
        }
    }

    /// <remarks>
    /// TODO: 尚未验证 <paramref name="loginType"/> 为 <see cref="LoginType.JXFW"/> 时能否正常登录
    /// </remarks>
    public virtual async Task<bool> Login(
        CookieContainer cookies,
        LoginInfo? loginInfo = null,
        LoginType loginType = LoginType.AuthServer,
        CancellationToken token = default)
    {
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            switch (loginType)
            {
                case LoginType.AuthServer:
                    {
                        response = await _authService.LoginAndAuth(cookies, IAuthService.SupportedServices.JXFW, loginInfo, token);
                        if (response is null) return false;
                        using var reader = new StreamReader(response.Content.ReadAsStream(token));
                        reader.ReadLine();  // skip
                        return reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == false;
                    }
                case LoginType.JXFW:
                    {
                        request = new(HttpMethod.Get, GSConst.AUTHSERVER_AUTH_PREFIX + GSConst.UNDER_GRADUATE_LOGIN);
                        response = await _client.SendAsync(cookies, request, token);
                        request.Dispose();
                        if (response.StatusCode == HttpStatusCode.OK)   // 需要登录
                        {
                            response.Dispose();
                            if (loginInfo is null)
                            {
                                Log.LoginFailed(_logger, null);
                                return false;
                            }
                            if (loginInfo.Captcha.Length != 4)
                            {
                                Log.CaptchaFailed(_logger, null);
                                return false;
                            }
                            StringBuilder sb = new();
                            sb.Append(loginInfo.Captcha)
                              .Append(loginInfo.Captcha)
                              .Append(loginInfo.Captcha)
                              .Append(loginInfo.Captcha);
                            var content = new Dictionary<string, string>
                            {
                                { "account", loginInfo.UserName },
                                { "pwd", Convert.ToHexString(_security.AesCbcEncrypt(loginInfo.Password.ToBytes(), sb.ToString().ToBytes(), _security.GenIV())) },
                                { "verifycode", loginInfo.Captcha },
                            };
                            request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.UNDER_LOGIN, content, GSConst.UNDER_LOGIN);
                            response = await _client.SendAsync(cookies, request, token);
                            request.Dispose();
                            return !(await response.Content.ReadAsStringAsync(token)).StartsWith("{\"code\":-");
                        }
                        else return true;
                    }
                default:
                    throw new InvalidDataException();
            }
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "登录", e);
            return false;
        }
        finally
        {
            request?.Dispose();
            response?.Dispose();
        }
    }

    public virtual async Task<Term?> GetTerm(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.UNDER_TERM, referer: GSConst.UNDER_TERM);
            using var response = await _client.SendAsync(cookies, request, token);
            string responseContent = await response.Content.ReadAsStringAsync(token);
            int index = responseContent.IndexOf("selected");
            return new(responseContent[(index - 2 - "202502".Length)..(index - 2)]);
            // responseContent 摘要:
            // <option value='202601' >2026秋季</option><option value='202502' selected>2026春季</option><option value='202501' >2025秋季</option>
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求学期", e);
            return null;
        }
    }

    public virtual async Task<List<Lesson>?> GetLessons(CookieContainer cookies, Term term, int? week = null, CancellationToken token = default)
    {
        try
        {
            return await GetData<Lesson, LessonDto, LessonDtoCollection>(
                cookies,
                GSConst.UNDER_LESSONS,
                new Dictionary<string, string>
                {
                    { "xnxqdm", $"{term.Code6}" },
                    { "zc", $"{week}" },
                    { "sort", "zc,xq,jcdm" },
                },
                AppJsonContext.Context.LessonDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求课表", e);
            return null;
        }
    }

    public virtual async Task<List<ExamSchedule>?> GetExamSchedule(CookieContainer cookies, Term term, CancellationToken token = default)
    {
        try
        {
            return await GetData<ExamSchedule, ExamScheduleDto, ExamScheduleDtoCollection>(
                cookies,
                GSConst.UNDER_EXAM_SCHEDULE,
                new Dictionary<string, string>
                {
                    { "xnxqdm", $"{term.Code6}" },
                    { "sort", "zc,xq,jcdm2" },
                },
                AppJsonContext.Context.ExamScheduleDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求考试安排", e);
            return null;
        }
    }

    public virtual async Task<List<CourseScore>?> GetCourseScore(CookieContainer cookies, Term? term = null, CancellationToken token = default)
    {
        try
        {
            var requestContent = new Dictionary<string, string>
                {
                    { "xnxqdm", $"{term?.Code6}" },
                    { "jhlxdm", "" },
                    { "sort", "xnxqdm" },
                };
            var result = await GetData<CourseScore, CourseScoreDto, CourseScoreDtoCollection>(
                cookies,
                GSConst.UNDER_COURSE_SCORE,
                requestContent,
                AppJsonContext.Context.CourseScoreDtoCollection,
                token);
            if (result != null && term is null)
            {
                HashSet<string> terms = [];
                foreach (var item in result)
                    terms.Add($"{item.Term.Code6}");
                foreach (var item in terms)
                {
                    requestContent["xnxqdm"] = item;
                    var tempResult = await GetData<CourseScore, CourseScoreDto, CourseScoreDtoCollection>(
                        cookies,
                        GSConst.UNDER_COURSE_SCORE,
                        requestContent,
                        AppJsonContext.Context.CourseScoreDtoCollection,
                        token);
                    if (tempResult != null)
                    {
                        foreach (var scoreItem in tempResult)
                        {
                            if (scoreItem.Name == "劳动教育")
                            {
                                result.Add(scoreItem);
                                goto END;
                            }
                        }
                    }
                }
            }
            END:
            return result;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求考试成绩", e);
            return null;
        }
    }

    public virtual async Task<List<CourseSel>?> GetCourseSelection(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            return await GetData<CourseSel, CourseSelDto, CourseSelDtoCollection>(
                cookies,
                GSConst.UNDER_COURSE_SEL,
                new Dictionary<string, string> { { "sort", "kcflmc" } },
                AppJsonContext.Context.CourseSelDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求可选课列表", e);
            return null;
        }
    }

    public virtual async Task<List<CourseSel>?> GetSelectedCourse(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            return await GetData<CourseSel, CourseSelDto, CourseSelDtoCollection>(
                cookies,
                GSConst.UNDER_COURSE_SEL_ED,
                new Dictionary<string, string> { { "sort", "kcflmc" } },
                AppJsonContext.Context.CourseSelDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求已选课列表", e);
            return null;
        }
    }

    public virtual async Task<List<Lesson>?> GetCourseTask(CookieContainer cookies, string code, CancellationToken token = default)
    {
        // 它比较特殊，不要使用 GetData() 方法
        try
        {
            List<Lesson>? r = null;
            var content = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "rows", $"{_itemPerRequest}" },
                    { "kcrwdm", code },
                    { "sort", "zc,xq,jcdm" },
                    { "order", "asc" },
                };
            for (int i = 1; i <= _maxPage; i++)
            {
                content["page"] = $"{i}";
                using var request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.UNDER_COURSE_TASK, content, GSConst.UNDER_COURSE_TASK);
                using HttpResponseMessage response = await _client.SendAsync(cookies, request, token);
                var temp = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ListLessonDto, token);
                List<Lesson>? tempResult = temp is null ? null : [..temp];
                if (r is null)
                {
                    r = tempResult;
                }
                else if (tempResult is not null)
                {
                    r.AddRange(tempResult);
                }
                if (tempResult is null || tempResult.Count < _itemPerRequest)
                {
                    break;
                }
            }
            return r;
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求课程任务", e);
            return null;
        }
    }

    public virtual async Task<List<GradingExamScore>?> GetGradingExamScore(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            return await GetData<GradingExamScore, GradingExamScoreDto, GradingExamScoreDtoCollection>(
                cookies,
                GSConst.UNDER_GRADING_EXAM_SCORE,
                new Dictionary<string, string> { { "sort", "xnxqdm,kssj" } },
                AppJsonContext.Context.GradingExamScoreDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求考级成绩", e);
            return null;
        }
    }

    public virtual async Task<List<AvaliableTeachingPlan>?> GetTeachingPlanList(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            return await GetData<AvaliableTeachingPlan, AvaliableTeachingPlanDto, AvaliableTeachingPlanDtoCollection>(
                cookies,
                GSConst.UNDER_TEACHING_PLAN_AVALIABLE,
                new Dictionary<string, string>{ { "sort", "nd" } },
                AppJsonContext.Context.AvaliableTeachingPlanDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求教学计划列表", e);
            return null;
        }
    }

    public virtual async Task<List<TeachingPlan>?> GetTeachingPlan(CookieContainer cookies, string planCode, CancellationToken token = default)
    {
        try
        {
            return await GetData<TeachingPlan, TeachingPlanDto, TeachingPlanDtoCollection>(
                cookies,
                GSConst.UNDER_TEACHING_PLAN_AVALIABLE,
                new Dictionary<string, string> { { "sort", "kkxqmc1" } },
                AppJsonContext.Context.TeachingPlanDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求教学计划", e);
            return null;
        }
    }

    public virtual async Task<List<SemesterReg>?> GetSemesterRegistration(CookieContainer cookies, CancellationToken token = default)
    {
        try
        {
            return await GetData<SemesterReg, SemesterRegDto, SemesterRegDtoCollection>(
                cookies,
                GSConst.UNDER_SEMESTER_REG,
                new Dictionary<string, string> { { "sort", "xnxqmc" } },
                AppJsonContext.Context.SemesterRegDtoCollection,
                token);
        }
        catch (Exception e)
        {
            Log.TryFailed(_logger, "请求学期注册", e);
            return null;
        }
    }
}
