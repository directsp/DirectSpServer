using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ApiInvokeParamsBatch
    {
        public SpCall[] SpCalls { get; set; }
        public ApiInvokeOptions InvokeOptions { get; set; }
    }
}
