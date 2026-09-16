#pragma warning disable IDE1006 // Naming Styles

namespace GDUTSharp.Shared.Type.DTO;

public class AuthServerCaptchaDto
{
    public string smallImage { get; set; } = string.Empty;

    public string bigImage { get; set; } = string.Empty;

    public int tagWidth { get; set; }

    public int yHeight { get; set; }

    public static implicit operator AuthServerCaptcha(AuthServerCaptchaDto dto)
    {
        return new AuthServerCaptcha
        {
            BigImage = dto.bigImage,
            SmallImage = dto.smallImage,
        };
    }
}

#pragma warning restore IDE1006 // Naming Styles
