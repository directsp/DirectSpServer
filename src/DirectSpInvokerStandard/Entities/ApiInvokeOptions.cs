using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ApiInvokeOptions
    {
        public string RequestId { get; set; }
        public string CaptchaId { get; set; }
        public string CaptchaCode { get; set; }
        public RecordsetFormat RecordsetFormat { get; set; } = RecordsetFormat.Json;
        public string RecordsetFileTitle { get; set; }
        public bool IsWithRecodsetDownloadUri { get; set; }
        public int? RecordIndex { get; set; }
        public int? RecordCount { get; set; }
    };

}
