using System.Net.Http.Json;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services;

public class JXFWService(ILogger<JXFWService> logger, ICommonClient client, IAuthService authService) : IJXFWService
{
    protected readonly ILogger<JXFWService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly IAuthService _authService = authService;

    public async virtual Task<bool> Login(LoginInfo? loginInfo = null)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await _authService.LoginAndAuth(IAuthService.SupportedServices.JXFW, loginInfo);
            if (response is null) return false;
            using var reader = new StreamReader(response.Content.ReadAsStream());
            reader.ReadLine();  // skip
            return reader.ReadLine()?.StartsWith("<!-- 移动端 -->") == false;
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
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_TERM, referer: GDUTConstant.UNDER_TERM);
            using var response = await _client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            int index = responseContent.IndexOf("selected");
            return responseContent[(index - 2 - "202502".Length)..(index - 2)];
            // responseContent 摘要:
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
            var requestContent = new Dictionary<string, string>
                {
                    { "xnxqdm", term },
                    { "zc", "" },
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "zc,xq,jcdm" },
                    { "order", "asc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_LESSONS, requestContent, GDUTConstant.UNDER_LESSONS);
            using HttpResponseMessage response = await _client.SendAsync(request);
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
            var requestContent = new Dictionary<string, string>
                {
                    { "xnxqdm", term },
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "zc,xq,jcdm2" },
                    { "order", "asc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_EXAM_SCHEDULE, requestContent, GDUTConstant.UNDER_EXAM_SCHEDULE);
            using HttpResponseMessage response = await _client.SendAsync(request);
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
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;
        try
        {
            var requestContent = new Dictionary<string, string>
                {
                    { "xnxqdm", term },
                    { "jhlxdm", "" },
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "xnxqdm" },
                    { "order", "asc" },
                };
            request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_COURSE_SCORE, requestContent, GDUTConstant.UNDER_COURSE_SCORE);
            response = await _client.SendAsync(request);
            request.Dispose();
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.CourseScoreDtoCollection);
            response.Dispose();
            if (result != null && term == "")
            {
                HashSet<string> terms = [];
                foreach (var item in result.rows)
                    terms.Add(item.xnxqmc);
                foreach (var item in terms)
                {
                    requestContent["xnxqdm"] = Helper.TermStringToInt6Digit(item).ToString();
                    request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_COURSE_SCORE, requestContent, GDUTConstant.UNDER_COURSE_SCORE);
                    response = await _client.SendAsync(request);
                    request.Dispose();
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
            }
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
            request?.Dispose();
            response?.Dispose();
        }
    }

    public async virtual Task<List<CourseSel>?> GetCourseSelection()
    {
        try
        {
            var requestContent = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "kcflmc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_COURSE_SEL, requestContent, GDUTConstant.UNDER_COURSE_SEL);
            using HttpResponseMessage response = await _client.SendAsync(request);
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
            var requestContent = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "kcflmc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_COURSE_SEL_ED, requestContent, GDUTConstant.UNDER_COURSE_SEL_ED);
            using HttpResponseMessage response = await _client.SendAsync(request);
            return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.CourseSelDtoCollection);
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
            var requestContent = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "rows", "300" },
                    { "kcrwdm", code },
                    { "sort", "zc,xq,jcdm" },
                    { "order", "asc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_COURSE_TASK, requestContent, GDUTConstant.UNDER_COURSE_TASK);
            using HttpResponseMessage response = await _client.SendAsync(request);
            return await response.Content.ReadFromJsonAsync(AppJsonContext.Context.LessonDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课程任务异常。 {Exception}", e);
            return null;
        }
    }

    public async Task<List<GradingExamScore>?> GetGradingExamScore()
    {
        try
        {
            var requestContent = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "rows", "300" },
                    { "sort", "xnxqdm,kssj" },
                    { "order", "asc" },
                };
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GDUTConstant.UNDER_GRADING_EXAM_SCORE, requestContent, GDUTConstant.UNDER_GRADING_EXAM_SCORE);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.GradingExamScoreDtoCollection);
            return result;
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考级成绩异常。 {Exception}", e);
            return null;
        }
    }
}
