namespace GDUTSharp.Shared;

public static class GDUTConstant
{
    #region 统一认证中心

    public const string AUTHSERVER_LOGIN = "https://authserver.gdut.edu.cn/authserver/login?type=userNameLogin";

    public const string AUTHSERVER_AUTH_PREFIX = "https://authserver.gdut.edu.cn/authserver/login?service=";

    public const string AUTHSERVER_LOGOUT = "https://authserver.gdut.edu.cn/authserver/logout";

    /// <remarks>需要在末尾添加学号</remarks>
    public const string AUTHSERVER_CHECK_CAPTCHA_PREFIX = "https://authserver.gdut.edu.cn/authserver/checkNeedCaptcha.htl?username=";

    #endregion

    #region 教学服务系统（本科生）

    /// <summary>本科生获取学期代码</summary>
    public const string UNDER_TERM = "https://jxfw.gdut.edu.cn/xsksap!ksapList.action";

    /// <summary>本科生获取课程安排</summary>
    public const string UNDER_LESSONS = "https://jxfw.gdut.edu.cn/xsgrkbcx!getDataList.action";

    /// <summary>本科生获取考试安排</summary>
    public const string UNDER_EXAM_SCHEDULE = "https://jxfw.gdut.edu.cn/xsksap!getDataList.action";

    /// <summary>本科生获取课程成绩</summary>
    public const string UNDER_COURSE_SCORE = "https://jxfw.gdut.edu.cn/xskccjxx!getDataList.action";

    /// <summary>本科生教学服务系统登录链接</summary>
    public const string UNDER_GRADUATE_LOGIN = "https://jxfw.gdut.edu.cn/new/ssoLogin";

    /// <summary>本科生选课可选列表</summary>
    public const string UNDER_COURSE_SEL = "https://jxfw.gdut.edu.cn/xsxklist!getDataList.action";

    /// <summary>本科生选课已选列表</summary>
    public const string UNDER_COURSE_SEL_ED = "https://jxfw.gdut.edu.cn/xsxklist!getXzkcList.action";

    /// <summary>本科生获取课程任务</summary>
    public const string UNDER_COURSE_TASK = "https://jxfw.gdut.edu.cn/xsxklist!getJxrlDataList.action";

    #endregion

    #region 图书馆

    /// <summary>图书馆登录链接</summary>
    public const string LIBRARY_LOGIN = "https://opac.gdut.edu.cn/sso-cas/cas/index/gdut";

    /// <summary>图书馆借阅列表</summary>
    public const string LIBRARY_LOAN_LIST = "https://opac.gdut.edu.cn/find/loanInfo/loanList";

    /// <summary>图书馆每日推荐</summary>
    public const string LIBRARY_DAILY_RECOMMEND = "https://opac.gdut.edu.cn/find/subscribe/dailyRecommend?";

    #endregion

    #region 通知公文网

    public const string NOTICE_BEFORE_LOGIN = "https://oas.gdut.edu.cn/seeyon/main.do";

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

    // 登录 ehall 大厅 pre 登录    TMA: 这是统一认证中心，ehall 是另外一个
    public const string EHALL_URL = "https://authserver.gdut.edu.cn/authserver/login?type=userNameLogin";

    // ehall 的个人信息接口，用来获取正确的学号    TMA: 同上
    public const string EHALL_USER_INFO = "https://authserver.gdut.edu.cn/personalInfo/common/getUserConf";

    // 后续补充这里即可
    public const string TEACHER_EHALL_LOGIN = "https://authserver.gdut.edu.cn/";

    // 图书馆图书检索  TODO
    public const string LIBRARY_SEARCH = "https://opac.gdut.edu.cn/find/unify/search";
}
