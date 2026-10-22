namespace GDUTSharp.Shared.Type;

public enum LoginType
{
    /// <summary>通过统一认证中心</summary>
    AuthServer,

    /// <summary>通过自有的登录方式</summary>
    Self,
}

/// <summary>支持统一认证中心认证的服务</summary>
public enum AuthServices
{
    AcademicAffairs,
    Library,
    Ehall,
}

/// <summary>从教学管理系统导出的课程数据文件类型</summary>
public enum LessonsFileType
{
    AutoDetect,
    // 以下四种是教学管理系统上显示的支持导出的格式，
    // 然而实际上 XLS 和 DOC 选项均导出相同文件，
    // CSV 和 TEXT 选项也导出相同文件。
    XLS,
    DOC,
    CSV,
    TEXT,
}