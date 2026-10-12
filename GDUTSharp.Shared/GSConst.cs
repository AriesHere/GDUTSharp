namespace GDUTSharp.Shared;

public static class GSConst
{
    /// <summary>本项目（GDUTSharp）的仓库地址</summary>
    public const string GDUTSharpRepo = "https://github.com/AriesHere/GDUTSharp";

    #region 统一认证中心

    public const string AuthServerLogin = "https://authserver.gdut.edu.cn/authserver/login?type=userNameLogin";

    /// <remarks>需要在末尾添加需要认证的服务地址</remarks>
    public const string AuthServerAuth = "https://authserver.gdut.edu.cn/authserver/login?service=";

    public const string AuthServerLogout = "https://authserver.gdut.edu.cn/authserver/logout";

    /// <remarks>需要在末尾添加学号</remarks>
    public const string AuthServerCheckCaptcha = "https://authserver.gdut.edu.cn/authserver/checkNeedCaptcha.htl?username=";

    public const string AuthServerGetCaptcha = "https://authserver.gdut.edu.cn/authserver/common/openSliderCaptcha.htl";

    public const string AuthServerVerifyCaptcha = "https://authserver.gdut.edu.cn/authserver/common/verifySliderCaptcha.htl";

    #endregion

    #region 教学服务系统（本科生）

    /// <summary>本科生教务系统自有登录</summary>
    public const string AcademicAffairsUnderSelfLogin = "https://jxfw.gdut.edu.cn/new/login";

    /// <summary>本科生获取验证码（用于自有登录）</summary>
    /// <remarks>在末尾补充 Unix 毫秒时间戳</remarks>
    public const string AcademicAffairsUnderCaptcha = "https://jxfw.gdut.edu.cn/yzm?d=";

    /// <summary>本科生获取学期代码</summary>
    public const string AcademicAffairsUnderTerm = "https://jxfw.gdut.edu.cn/xsksap!ksapList.action";

    /// <summary>本科生获取课程安排</summary>
    public const string AcademicAffairsUnderLessons = "https://jxfw.gdut.edu.cn/xsgrkbcx!getDataList.action";

    /// <summary>本科生获取考试安排</summary>
    public const string AcademicAffairsUnderExamSchedule = "https://jxfw.gdut.edu.cn/xsksap!getDataList.action";

    /// <summary>本科生获取课程成绩</summary>
    public const string AcademicAffairsUnderCourseScore = "https://jxfw.gdut.edu.cn/xskccjxx!getDataList.action";

    /// <summary>本科生教学服务系统统一认证登录链接</summary>
    /// <remarks>需要拼在 <see cref="AuthServerAuth"/> 末尾</remarks>
    public const string AcademicAffairsUnderLogin = "https://jxfw.gdut.edu.cn/new/ssoLogin";

    /// <summary>本科生选课可选列表</summary>
    public const string AcademicAffairsUnderCourseSel = "https://jxfw.gdut.edu.cn/xsxklist!getDataList.action";

    /// <summary>本科生选课已选列表</summary>
    public const string AcademicAffairsUnderCourseSelected = "https://jxfw.gdut.edu.cn/xsxklist!getXzkcList.action";

    /// <summary>本科生获取课程任务</summary>
    public const string AcademicAffairsUnderCourseTask = "https://jxfw.gdut.edu.cn/xsxklist!getJxrlDataList.action";

    /// <summary>本科生获取考级成绩</summary>
    public const string AcademicAffairsUnderGradingExamScore = "https://jxfw.gdut.edu.cn/xskjcjxx!getDataList.action";

    /// <summary>本科生获取可用教学计划</summary>
    public const string AcademicAffairsUnderAvailableTeachingPlan = "https://jxfw.gdut.edu.cn/xsjxjhxx!getDataList1.action";

    /// <summary>本科生获取教学计划详情</summary>
    /// <remarks>在末尾补充教学计划代码</remarks>
    public const string AcademicAffairsUnderTeachingPlanDetail = "https://jxfw.gdut.edu.cn/xsjxjhxx!getKcDataList.action?jxjhdm=";

    /// <summary>本科生获取学期注册信息</summary>
    public const string AcademicAffairsUnderSemesterReg = "https://jxfw.gdut.edu.cn/xsxqzccx!getDataList.action";

    #endregion

    #region 图书馆

    /// <summary>图书馆统一认证中心登录链接</summary>
    public const string LibraryLogin = "https://opac.gdut.edu.cn/sso-cas/cas/index/gdut";

    /// <summary>图书馆借阅列表</summary>
    public const string LibraryLoanList = "https://opac.gdut.edu.cn/find/loanInfo/loanList";

    /// <summary>图书馆每日推荐</summary>
    public const string LibraryDalyRecommend = "https://opac.gdut.edu.cn/find/subscribe/dailyRecommend?";

    #endregion

    #region 通知公文网

    public const string NOTICE_BASE = "https://oas.gdut.edu.cn";

    public const string NOTICE_BEFORE_LOGIN = "https://oas.gdut.edu.cn/seeyon/main.do";

    /// <summary>分类数据</summary>
    public const string NOTICE_CATEGORIES = "https://oas.gdut.edu.cn/seeyon/ggIP.do?method=portalSeachIndex";

    /// <summary>某分类下的通知数据</summary>
    public const string NOTICE_CATEGORIES_GET = "https://oas.gdut.edu.cn/seeyon/ajax.do?method=ajaxAction&managerName=newsDataManager";

    /// <summary>通知详情</summary>
    /// <remarks>在末尾补充通知的 id</remarks>
    public const string NOTICE_DETAIL = "https://oas.gdut.edu.cn/seeyon/newsData.do?method=newsView&newsId=";

    #endregion

    #region 体质测试平台

    public const string SportsTestLogin = "http://tzcs.gdut.edu.cn/Src/PlatForm/User/Login.aspx?Mode=2";

    public const string SportsTestGetCaptcha = "http://tzcs.gdut.edu.cn/Src/PlatForm/GetCheckCode.aspx";

    /// <summary>个人主页</summary>
    public const string SportsTestPersonal = "http://tzcs.gdut.edu.cn/Src/PlatForm/Personal/Index.aspx";

    /// <summary>历史成绩</summary>
    /// <remarks>在末尾补上年份，如“2026”</remarks>
    public const string SportsTestScore = "http://tzcs.gdut.edu.cn/Src/PlatForm/Personal/PHTest_StudentScore_H.aspx?DataYear=";

    #endregion

    // =============== 以下均暂时未使用 ===============

    // 研究生登录链接
    public const string GRADUATE_EHALL_LOGIN = "https://authserver.gdut.edu.cn/authserver/login?service=https://yjsxt.gdut.edu.cn/gsapp/sys/yjsemaphome/portal/index.do";

    // 成绩登录授权
    public const string GRADUATE_EHALL_SCORE_LOGIN = "https://yjsxt.gdut.edu.cn/gsapp/sys/wdcjapp/*default/index.do#/wdcj";

    // 课表登录授权
    public const string GRADUATE_KB_LOGIN = "https://yjsxt.gdut.edu.cn/gsapp/sys/wdkbapp/*default/index.do";
    // 研究生学期信息
    public const string GRADUATE_SEMESTER = "https://yjsxt.gdut.edu.cn/gsapp/sys/wdkbapp/modules/xskcb/kfdxnxqcx.do";

    // 研究生课表接口，需要先拿到学期信息
    public const string GRADUATE_KB = "https://yjsxt.gdut.edu.cn/gsapp/sys/wdkbapp/modules/xskcb/xspkjgcx.do?XNXQDM=20221&*order=<*order>";
    // 考试成绩
    public const string GRADUATE_EXAM = "https://yjsxt.gdut.edu.cn/gsapp/sys/wdcjapp/modules/wdcj/xscjcx.do";

    // 从 ehall 获得用户个人信息，用来测试是否登录成功
    public const string GRADUATE_USER_INFO = "https://ehall.gdut.edu.cn/gsapp/sys/wdkbapp/wdkcb/initXsxx.do?XH=";

    // 登录 ehall 大厅 pre 登录
    public const string EHALL_URL = "https://authserver.gdut.edu.cn/authserver/login?type=userNameLogin";

    // ehall 的个人信息接口，用来获取正确的学号
    public const string EHALL_USER_INFO = "https://authserver.gdut.edu.cn/personalInfo/common/getUserConf";

    // 后续补充这里即可
    public const string TEACHER_EHALL_LOGIN = "https://authserver.gdut.edu.cn/";

    // TODO： 图书馆图书检索
    public const string LIBRARY_SEARCH = "https://opac.gdut.edu.cn/find/unify/search";
}
