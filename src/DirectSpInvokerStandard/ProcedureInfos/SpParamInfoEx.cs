
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp.ProcedureInfos
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpParamInfoEx
    {
        public SpSignType SignType { get; set; }
    }
}
