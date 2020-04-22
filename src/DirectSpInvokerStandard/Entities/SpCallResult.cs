using System.Collections.Generic;
using System.Text.Json;

namespace DirectSp
{
    public class SpCallResult : Dictionary<string, object>
    {
        public T ConvertParams<T>()
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(this));
        }

        public T ConvertParam<T>(string paramName)
        {
            return ContainsKey(paramName) ? JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(this[paramName])) : default;
        }

        public T ConvertRecordset<T>() where T : class
        {
            return Recordset != null ? JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Recordset)) : null;
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
