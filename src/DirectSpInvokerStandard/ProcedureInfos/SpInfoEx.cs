using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace DirectSp.ProcedureInfos
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpInfoEx
    {
        public SpDataAccessMode DataAccessMode { get; set; } = SpDataAccessMode.Write;
        public bool IsBatchAllowed { get; set; }
        public SpCaptchaMode CaptchaMode { get; set; } = SpCaptchaMode.Manual;
        public IDictionary<string, SpParamInfoEx> Params { get; set; } = new Dictionary<string, SpParamInfoEx>();
        public IDictionary<string, SpFieldInfo> Fields { get; set; } = new Dictionary<string, SpFieldInfo>();
        public int CommandTimeout { get; set; } = 30;
    }
}
