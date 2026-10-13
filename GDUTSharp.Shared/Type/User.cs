using System.Text.Json.Serialization;
using GDUTSharp.Shared.Attributes;

namespace GDUTSharp.Shared.Type
{
    public class LoginInfo
    {
        public required string UserName { get; set; }

        public required string Password { get; set; }

        public string Captcha { get; set; } = string.Empty;
    }

    /// <summary>统一认证中心验证码（滑块验证码）</summary>
    /// <remarks><see cref="SmallImage"/> 和 <see cref="BigImage"/> 都是 png 文件</remarks>
    [OverrideToString]
    public partial class AuthServerCaptcha
    {
        [JsonPropertyName("smallImage")]
        public byte[] SmallImage { get; set; } = [];

        [JsonPropertyName("bigImage")]
        public byte[] BigImage { get; set; } = [];
        public byte[] SafeSecure => SmallImage[^16..];
    }

#pragma warning disable IDE1006 // Naming Styles

    // 以下部分仅用于发送至服务端，不用存储

    public class SliderTrackDto
    {
        /// <summary>横向位移（相对于第一个点）</summary>
        public int a { get; set; }
        /// <summary>纵向位移（相对于第一个点）</summary>
        public int b { get; set; }
        /// <summary>距上一点耗时(ms)</summary>
        public int c { get; set; }
    }

    /// <remarks>
    /// <see cref="tracks"/> 的第一个元素必定是 { a = 0, b = 0, c = 0 }，表示起点，后续点均相对于它。
    /// <see cref="canvasLength"/> 是大图长度（可以是缩放后的长度），<see cref="moveLength"/>
    /// 是最后一个点的 <see cref="SliderTrackDto.a"/>，必为正值。如果 <see cref="canvasLength"/>
    /// 是缩放后的长度，则 <see cref="tracks"/> 中各个点的位移应当是缩放后的位移，
    /// <see cref="moveLength"/> 也应当是缩放后的长度。
    /// </remarks>
    public class SliderPayloadDto
    {
        public int canvasLength { get; set; }
        public int moveLength { get; set; }
        public List<SliderTrackDto> tracks { get; set; } = [];
    }
}

#pragma warning restore IDE1006 // Naming Styles
