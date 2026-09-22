using GDUTSharp.Shared.Attributes;

namespace GDUTSharp.Shared.Type;

/// <summary>学期注册</summary>
[OverrideToString("SemesterRegistration")]
public partial class SemesterReg
{
    public string Term { get; set; } = string.Empty;

    /// <summary>注册状态</summary>
    public string Status { get; set; } = string.Empty;
}
