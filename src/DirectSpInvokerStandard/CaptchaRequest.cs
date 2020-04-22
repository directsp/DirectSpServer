
using System.Text.Json.Serialization;

namespace DirectSp
{
    public class CaptchaRequest
    {
        [JsonPropertyName("captchaId")]
        public string Id { get; set; }

        [JsonPropertyName("captchaImage")]
        public byte[] ImageBuffer { get; set; }
    }
}