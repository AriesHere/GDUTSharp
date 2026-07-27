using System.Diagnostics.CodeAnalysis;

namespace GDUTSharp.Type
{
    public class LoginInfo
    {
        [NotNull] public required string UserName { get; set; }

        [NotNull] public required string Password { get; set; }

        public Role Role => (Role)(UserName[0] - '0');
    }

    public enum Role
    {
        UNDER_GRADUATE = 3,
        GRADUATE = 2,
        TRACHER = 0,
    }
}
