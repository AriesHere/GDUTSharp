using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using GDUTSharp.Shared.Json;
using GDUTSharp.Shared.Type;
using GDUTSharp.Shared.Type.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    IAuthService authService
    ) : IJXFWService
{
    protected readonly ILogger<JXFWService> _logger = logger;
    protected readonly ICommonClient _client = client;
    protected readonly IAuthService _authService = authService;
    protected readonly int _maxPage = options.Value.MaxPage;
    protected readonly int _itemPerRequest = options.Value.ItemPerRequest;

    protected async virtual Task<List<TResult>?> GetData<TResult, TDto, TDtoCollection>(
        string url,
        Dictionary<string, string> requestContent,
        JsonTypeInfo<TDtoCollection> jsonTypeInfo
        ) where TDtoCollection : DtoCollectionBase<TResult, TDto>
    {
        List<TResult>? r = null;
        requestContent.AddIfNotExist("rows", $"{_itemPerRequest}").AddIfNotExist("page", "1").AddIfNotExist("order", "asc");
        for (int i = 1; i <= _maxPage; i++)
        {
            requestContent["page"] = $"{i}";
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, url, requestContent, url);
            using HttpResponseMessage response = await _client.SendAsync(request);
            var temp = await response.Content.ReadFromJsonAsync(jsonTypeInfo);
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

    public async virtual Task<Term?> GetTerm()
    {
        try
        {
            using var request = ICommonClient.CreateRequest(HttpMethod.Post, GSConst.UNDER_TERM, referer: GSConst.UNDER_TERM);
            using var response = await _client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            int index = responseContent.IndexOf("selected");
            return new(responseContent[(index - 2 - "202502".Length)..(index - 2)]);
            // responseContent 摘要:
            // <option value='202601' >2026秋季</option><option value='202502' selected>2026春季</option><option value='202501' >2025秋季</option>
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求学期异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<Lesson>?> GetLessons(Term term, int? week = null)
    {
        try
        {
            return await GetData<Lesson, LessonDto, LessonDtoCollection>(
                GSConst.UNDER_LESSONS,
                new Dictionary<string, string>
                {
                    { "xnxqdm", $"{term.Code6}" },
                    { "zc", $"{week}" },
                    { "sort", "zc,xq,jcdm" },
                },
                AppJsonContext.Context.LessonDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课表异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<ExamSchedule>?> GetExamSchedule(Term term)
    {
        try
        {
            return await GetData<ExamSchedule, ExamScheduleDto, ExamScheduleDtoCollection>(
                GSConst.UNDER_EXAM_SCHEDULE,
                new Dictionary<string, string>
                {
                    { "xnxqdm", $"{term.Code6}" },
                    { "sort", "zc,xq,jcdm2" },
                },
                AppJsonContext.Context.ExamScheduleDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试安排异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<CourseScore>?> GetCourseScore(Term? term = null)
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
                GSConst.UNDER_COURSE_SCORE,
                requestContent,
                AppJsonContext.Context.CourseScoreDtoCollection);
            if (result != null && term is null)
            {
                HashSet<string> terms = [];
                foreach (var item in result)
                    terms.Add($"{item.Term.Code6}");
                foreach (var item in terms)
                {
                    requestContent["xnxqdm"] = item;
                    var tempResult = await GetData<CourseScore, CourseScoreDto, CourseScoreDtoCollection>(
                        GSConst.UNDER_COURSE_SCORE,
                        requestContent,
                        AppJsonContext.Context.CourseScoreDtoCollection);
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
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考试成绩异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<CourseSel>?> GetCourseSelection()
    {
        try
        {
            return await GetData<CourseSel, CourseSelDto, CourseSelDtoCollection>(
                GSConst.UNDER_COURSE_SEL,
                new Dictionary<string, string> { { "sort", "kcflmc" } },
                AppJsonContext.Context.CourseSelDtoCollection);
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
            return await GetData<CourseSel, CourseSelDto, CourseSelDtoCollection>(
                GSConst.UNDER_COURSE_SEL_ED,
                new Dictionary<string, string> { { "sort", "kcflmc" } },
                AppJsonContext.Context.CourseSelDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求已选课列表异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<Lesson>?> GetCourseTask(string code)
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
                using HttpResponseMessage response = await _client.SendAsync(request);
                var temp = await response.Content.ReadFromJsonAsync(AppJsonContext.Context.ListLessonDto);
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
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求课程任务异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<GradingExamScore>?> GetGradingExamScore()
    {
        try
        {
            return await GetData<GradingExamScore, GradingExamScoreDto, GradingExamScoreDtoCollection>(
                GSConst.UNDER_GRADING_EXAM_SCORE,
                new Dictionary<string, string> { { "sort", "xnxqdm,kssj" } },
                AppJsonContext.Context.GradingExamScoreDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求考级成绩异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<AvaliableTeachingPlan>?> GetTeachingPlanList()
    {
        try
        {
            return await GetData<AvaliableTeachingPlan, AvaliableTeachingPlanDto, AvaliableTeachingPlanDtoCollection>(
                GSConst.UNDER_TEACHING_PLAN_AVALIABLE,
                new Dictionary<string, string>{ { "sort", "nd" } },
                AppJsonContext.Context.AvaliableTeachingPlanDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求教学计划列表异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<TeachingPlan>?> GetTeachingPlan(string planCode)
    {
        try
        {
            return await GetData<TeachingPlan, TeachingPlanDto, TeachingPlanDtoCollection>(
                GSConst.UNDER_TEACHING_PLAN_AVALIABLE,
                new Dictionary<string, string> { { "sort", "kkxqmc1" } },
                AppJsonContext.Context.TeachingPlanDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求教学计划异常。 {Exception}", e);
            return null;
        }
    }

    public async virtual Task<List<SemesterReg>?> GetSemesterRegistration()
    {
        try
        {
            return await GetData<SemesterReg, SemesterRegDto, SemesterRegDtoCollection>(
                GSConst.UNDER_SEMESTER_REG,
                new Dictionary<string, string> { { "sort", "xnxqmc" } },
                AppJsonContext.Context.SemesterRegDtoCollection);
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error)) _logger.LogError("请求教学计划异常。 {Exception}", e);
            return null;
        }
    }
}
