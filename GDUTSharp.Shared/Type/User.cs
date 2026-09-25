using System.Text;
using GDUTSharp.Shared.Attributes;

namespace GDUTSharp.Shared.Type
{
    public class LoginInfo
    {
        public required string UserName { get; set; }

        public required string Password { get; set; }

        public string Captcha { get; set; } = string.Empty;
    }

    /// <summary>统一认证中心验证码</summary>
    /// <remarks><see cref="SmallImage"/> 和 <see cref="BigImage"/> 都是经过 Base64 编码的 png 文件</remarks>
    [OverrideToString]
    public partial class AuthServerCaptcha
    {
        public string SmallImage { get; set; } = string.Empty;
        public string BigImage { get; set; } = string.Empty;
        public string SateSecure
        {
            get
            {
                byte[] d = Convert.FromBase64String(SmallImage);
                return Encoding.Latin1.GetString(d, d.Length - 16, 16);
            }
        }
    }

#pragma warning disable IDE1006 // Naming Styles

    // 以下部分仅用于发送至服务端，不用存储

    public class SliderTrackDto
    {
        public double a { get; set; }   // 横向位移
        public double b { get; set; }   // 纵向位移
        public double c { get; set; }   // 距上一点耗时(ms)
    }

    public class SliderPayloadDto
    {
        public double canvasLength { get; set; }
        public double moveLength { get; set; }
        public List<SliderTrackDto> tracks { get; set; } = [];
    }
}

#pragma warning restore IDE1006 // Naming Styles
