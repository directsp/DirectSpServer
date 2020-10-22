using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ApiInvokeParams
    {
        public SpCall SpCall { get; set; }
        public ApiInvokeOptions InvokeOptions { get; set; } = new ApiInvokeOptions();
    }
}
