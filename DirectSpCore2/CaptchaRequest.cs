using Newtonsoft.Json;

namespace DirectSp.Core
{
    public class CaptchaRequest
    {
        [JsonProperty("CaptchaId")]
        public string Id { get; set; }

        [JsonProperty("CaptchaImage")]
        public byte[] ImageBuffer { get; set; }
    }
}