using Newtonsoft.Json;

namespace DirectSp.Core.Entities
{
    public class Captcha
    {
        [JsonProperty("CaptchaId")]
        public string Id { get; set; }

        [JsonProperty("CaptchaImage")]
        public byte[] Image { get; set; }
    }
}
