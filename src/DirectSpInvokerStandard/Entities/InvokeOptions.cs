using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace DirectSp
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class InvokeOptions
    {
        public bool IsLocalRequest { get; set; } = true;
        public IPAddress RequestRemoteIp { get; set; }
        public string AuthUserId { get; set; }
        public string Audience { get; set; }
        public ApiInvokeOptions ApiInvokeOptions { get; set; } = new ApiInvokeOptions();
        public string RecordsetDownloadUrlTemplate { get; set; } //should contain {id} and {filename}
        public string UserAgent { get; set; }
    }
}
