using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DirectSp.ProcedureInfos
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpParamInfo
    {
        public string ParamName { get; set; }
        public string SystemTypeName { get; set; }
        public string UserTypeName { get; set; }
        public int Length { get; set; }
        public bool IsOutput { get; set; }
        public object DefaultValue { get; set; }
        public bool IsOptional { get; set; }
    }
}
