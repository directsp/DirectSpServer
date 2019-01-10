using Newtonsoft.Json;

namespace DirectSp
{
    public class CaptchaRequest
    {
        [JsonProperty("captchaId")]
        public string Id { get; set; }

        [JsonProperty("captchaImage")]
        public byte[] ImageBuffer { get; set; }
    }
}