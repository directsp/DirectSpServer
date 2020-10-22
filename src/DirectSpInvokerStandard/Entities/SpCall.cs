using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace DirectSp
{

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpCall
    {
        public string Method { get; set; }
        public IDictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
    }
}
