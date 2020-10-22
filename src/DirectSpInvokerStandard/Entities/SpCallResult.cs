using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace DirectSp
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SpCallResult : Dictionary<string, object>
    {
        public T ConvertParams<T>()
        {
            return JToken.FromObject(this).ToObject<T>();
        }

        public T ConvertParam<T>(string paramName)
        {
            return ContainsKey(paramName) ? JToken.FromObject(this[paramName]).ToObject<T>() : default;
        }

        public T ConvertRecordset<T>() where T : class
        {
            return Recordset != null ? JToken.FromObject(Recordset).ToObject<T>() : null;
        }

        public IDictionary<string, string> RecordsetFields => (IDictionary<string, string>)this["RecordsetFields"];

        public IEnumerable<IDictionary<string, object>> Recordset
        {
            get => (IEnumerable<IDictionary<string, object>>)this["Recordset"];
            internal set => this["Recordset"] = value;
        }

        public string RecordsetText
        {
            get => ContainsKey("RecordsetText") ? this["RecordsetText"] as string : null;
            internal set
            {
                if (value != null)
                    this["RecordsetText"] = value;
                else if (ContainsKey("RecordsetText"))
                    Remove("RecordsetText");
            }

        }

        public object ReturnValue
        {
            get => this["returnValue"];
            internal set => this["returnValue"] = value;
        }

        public string RecordsetUri
        {
            get => ContainsKey("RecordsetUri") ? this["RecordsetUri"] as string : null;
            internal set
            {
                if (value != null)
                    this["RecordsetUri"] = value;
                else if (ContainsKey("RecordsetUri"))
                    Remove("RecordsetUri");
            }
        }
    }
}
