namespace GDUTSharp
{
    public static class Constant
    {
        #region 已用

        public const string AUTHSERVER_LOGIN_URL = "https://authserver.gdut.edu.cn/authserver/login?type=userNameLogin";

        public const string AUTHSERVER_AUTH_Prefix = "https://authserver.gdut.edu.cn/authserver/login?service=";

        // 本科生获取学期链接
        public const string UNDER_CLAZZ_TERM = "https://jxfw.gdut.edu.cn/xsksap!ksapList.action";

        public const string UNDER_CLAZZ = "https://jxfw.gdut.edu.cn/xsgrkbcx!getDataList.action";
        
        // 本科生考试安排
        public const string UNDER_EXAM = "https://jxfw.gdut.edu.cn/xsksap!getDataList.action";

        // 本科生获得成绩接口
        public const string UNDER_EXAM_SCORE = "https://jxfw.gdut.edu.cn/xskccjxx!getDataList.action";

        // 本科生教学服务系统登录链接
        public const string UNDER_GRADUATE_LOGIN = "https://jxfw.gdut.edu.cn/new/ssoLogin";

        // 本科生选课列表
        public const string UNDER_COURSE_SEL = "https://jxfw.gdut.edu.cn/xsxklist!getDataList.action";

        // 本科生选课已选列表
        public const string UNDER_COURSE_SEL_ED = "https://jxfw.gdut.edu.cn/xsxklist!getXzkcList.action";

        // 本科生课程任务
        public const string UNDER_COURSE_TASK = "https://jxfw.gdut.edu.cn/xsxklist!getJxrlDataList.action";

        #endregion

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
    }
}
