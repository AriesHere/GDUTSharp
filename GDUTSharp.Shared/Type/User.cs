using System.Diagnostics.CodeAnalysis;

namespace GDUTSharp.Shared.Type
{
    public partial class LoginInfo
    {
        /// <summary>学号</summary>
        [NotNull] public required string UserName { get; set; }

        [NotNull] public required string Password { get; set; }

        public Role Role => UserName.Length > 0 ? (Role)(UserName[0] - '0') : Role.UNKNOWN;
    }

    public enum Role
    {
        UNDER_GRADUATE = 3,
        GRADUATE = 2,
        TRACHER = 0,
        UNKNOWN = -1,
    }
}
