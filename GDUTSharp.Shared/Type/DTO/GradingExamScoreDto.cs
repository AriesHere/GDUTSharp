namespace GDUTSharp.Shared.Type.DTO;

#pragma warning disable IDE1006 // Naming Styles

public class GradingExamScoreDtoCollection : DtoCollectionBase<GradingExamScore, GradingExamScoreDto>
{
    public override List<GradingExamScore> Convert() => [..this];
}

public class GradingExamScoreDto
{
    public string xnxqmc { get; set; } = string.Empty;

    /// <summary>考级课程编号，如“CET6”</summary>
    public string kjkcbh { get; set; } = string.Empty;

    /// <summary>考级课程名称，如”英语六级“</summary>
    public string kjkcmc { get; set; } = string.Empty;

    /// <summary>考试时间</summary>
    public DateOnly kssj { get; set; } = DateOnly.MinValue;

    /// <summary>准考证号</summary>
    public string zkzh { get; set; } = string.Empty;

    public string zcj { get; set; } = string.Empty;

    /// 没考过除四六级以外的东西，在解析数据时看到了这样的东西（XXX表示数字）：
    /// {
    ///     ...
    ///     "xm1cj": "XXX",     // 听力成绩
    ///     "xm2cj": "XXX",     // 阅读成绩
    ///     "xm3cj": "XXX",     // 写作成绩
    ///     "xm4cj": "",        // 未知
    ///     "xm5cj": "XXX",      // 口语成绩
    ///     ...
    /// }

    public string xm1cj { get; set; } = string.Empty;
   
    public string xm2cj { get; set; } = string.Empty;
   
    public string xm3cj { get; set; } = string.Empty;
   
    public string xm4cj { get; set; } = string.Empty;
   
    public string xm5cj { get; set; } = string.Empty;

    protected static string Check(string raw) => raw.Length == 0 ? raw : raw[0] == '-' ? string.Empty : raw;

    public static implicit operator GradingExamScore(GradingExamScoreDto dto)
    {
        var r = new GradingExamScore
        {
            Term = new(dto.xnxqmc),
            GradingExamCode = dto.kjkcbh,
            GradingExamName = dto.kjkcmc,
            Date = dto.kssj,
            PermissionNumber = dto.zkzh,
            Score = dto.zcj,
        };
        if (dto.kjkcbh == "CET4" || dto.kjkcbh == "CET6")
        {
            //              [听力成绩,          阅读成绩,          写作成绩,          口语成绩]
            r.ScoreDetail = [Check(dto.xm1cj), Check(dto.xm2cj), Check(dto.xm3cj), Check(dto.xm5cj)];
        }
        return r;
    }
}

#pragma warning restore IDE1006 // Naming Styles
