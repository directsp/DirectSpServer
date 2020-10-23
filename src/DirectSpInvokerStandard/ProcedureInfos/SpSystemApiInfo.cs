using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.ProcedureInfos
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpSystemApiInfo
    {
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public SpInfo[] ProcInfos { get; set; }
    }
}
