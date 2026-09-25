namespace GDUTSharp.Shared.Type;

/// <summary>体质测试成绩</summary>
/// <remarks>
/// 末尾为 _R 的是原始成绩，末尾为 _RS 的是原始得分，末尾为 _WS 的是加权得分<br/>
/// 性别不同，需要测试的项目不同
/// </remarks>
[Attributes.OverrideToString]
public partial class SportsTestScore
{
    /// <summary>体质测试测试年份</summary>
    public string Year { get; set; } = string.Empty;

    /// <summary>姓名</summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>学号</summary>
    public string StudentId { get; set; } = string.Empty;

    #region 体型

    /// <summary>身高，单位cm</summary>
    public float Height_R { get; set; }

    /// <summary>体重，单位kg</summary>
    public float Weight_R { get; set; }

    /// <summary>身高体重原始得分</summary>
    public int Figure_RS { get; set; }

    /// <summary>身高体重加权得分</summary>
    public float Figure_WS { get; set; }

    #endregion

    #region 肺活量

    /// <summary>肺活量，单位mL</summary>
    public int VitalCapacity_R { get; set; }

    /// <summary>肺活量原始得分</summary>
    public int VitalCapacity_RS { get; set; }

    /// <summary>肺活量加权得分</summary>
    public float VitalCapacity_WS { get; set; }

    #endregion

    #region 50 米跑

    /// <summary>50 米跑</summary>
    public TimeOnly Meter50_R { get; set; }

    /// <summary>50 米跑原始得分</summary>
    public int Meter50_RS { get; set; }

    /// <summary>50 米跑加权得分</summary>
    public float Meter50_WS { get; set; }

    #endregion

    #region 坐位体前屈

    /// <summary>坐位体前屈，单位cm</summary>
    public float SitAndReach_R { get; set; }

    /// <summary>坐位体前屈原始得分</summary>
    public int SitAndReach_RS { get; set; }

    /// <summary>坐位体前屈加权得分</summary>
    public float SitAndReach_WS { get; set; }

    #endregion

    #region 立定跳远

    /// <summary>立定跳远，单位cm</summary>
    public float StandingLongJump_R { get; set; }

    /// <summary>立定跳远原始得分</summary>
    public int StandingLongJump_RS { get; set; }

    /// <summary>立定跳远加权得分</summary>
    public float StandingLongJump_WS { get; set; }

    #endregion

    #region 跳绳

    /// <summary>跳绳，单位次</summary>
    public int RopeSkipping_R { get; set; }

    /// <summary>跳绳原始得分</summary>
    public int RopeSkipping_RS { get; set; }

    /// <summary>跳绳加权得分</summary>
    public float RopeSkipping_WS { get; set; }

    #endregion

    #region 引体向上

    /// <summary>引体向上，单位次</summary>
    public int PullUp_R { get; set; }

    /// <summary>引体向上原始得分</summary>
    public int PullUp_RS { get; set; }

    /// <summary>引体向上加权得分</summary>
    public float PullUp_WS { get; set; }

    #endregion

    #region 仰卧起坐

    /// <summary>仰卧起坐，单位次</summary>
    public int SitUp_R { get; set; }

    /// <summary>仰卧起坐原始得分</summary>
    public int SitUp_RS { get; set; }

    /// <summary>仰卧起坐加权得分</summary>
    public float SitUp_WS { get; set; }

    #endregion

    #region 1000 米跑

    /// <summary>1000 米跑</summary>
    public TimeOnly Meter1k_R { get; set; }

    /// <summary>1000 米跑原始得分</summary>
    public int Meter1k_RS { get; set; }

    /// <summary>1000 米跑加权得分</summary>
    public float Meter1k_WS { get; set; }

    #endregion

    #region 800 米跑

    /// <summary>800 米跑</summary>
    public TimeOnly Meter800_R { get; set; }

    /// <summary>800 米跑原始得分</summary>
    public int Meter800_RS { get; set; }

    /// <summary>800 米跑加权得分</summary>
    public float Meter800_WS { get; set; }

    #endregion

    #region 50米×8往返跑

    /// <summary>50米×8往返跑</summary>
    public TimeOnly RoundTrip_R { get; set; }

    /// <summary>50米×8往返跑原始得分</summary>
    public int RoundTrip_RS { get; set; }

    /// <summary>50米×8往返跑加权得分</summary>
    public float RoundTrip_WS { get; set; }

    #endregion

    /// <summary>加权总分</summary>
    public float Score { get; set; }
}
