using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DirectSp.Core.Entities
{
    public class SpCallResult : Dictionary<string, object>
    {
        public T MapParams<T>()
        {
            return JToken.FromObject(this).ToObject<T>();
        }

        public T MapParam<T>(string paramName) where T : class
        {
            return ContainsKey(paramName) ? JToken.FromObject(this[paramName]).ToObject<T>() : null;
        }

        public T MapRecordset<T>() where T : class
        {
            return Recordset != null ? JToken.FromObject(Recordset).ToObject<T>() : null;
        }

        public IDictionary<string, string> RecordsetFields { get { return (IDictionary<string, string>)this["RecordsetFields"]; } }

        public IEnumerable<IDictionary<string, object>> Recordset
        {
            get { return (IEnumerable<IDictionary<string, object>>)this["Recordset"]; }
            internal set { this["Recordset"] = value; }
        }

        public string RecordsetText
        {
            get { return ContainsKey("RecordsetText") ? this["RecordsetText"] as string : null; }
            internal set
            {
                if (value != null)
                    this["RecordsetText"] = value;
                else if (ContainsKey("RecordsetText"))
                    Remove("RecordsetText");
            }

        }

        public int ReturnValue { get { return (int)this["ReturnValue"]; } }

        public string RecordsetUri
        {
            get { return ContainsKey("RecordsetUri") ? this["RecordsetUri"] as string : null; }
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
