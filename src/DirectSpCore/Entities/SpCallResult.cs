using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.Core.Entities
{
    public class SpCallResult : Dictionary<string, object>
    {
        public IDictionary<string, string> RecordsetFields { get { return (IDictionary<string, string>)this["RecordsetFields"]; } }

        public IEnumerable<IDictionary<string, object>> Recordset
        {
            get { return (IEnumerable<IDictionary<string, object>>)this["Recordset"]; }
            set
            {
                this["Recordset"] = value;
            }
        }

        public string RecordsetText
        {
            get { return ContainsKey("RecordsetText") ? this["RecordsetText"] as string : null; }
            set
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
            set
            {
                if (value != null)
                    this["RecordsetUri"] = value;
                else if (ContainsKey("RecordsetUri"))
                    Remove("RecordsetUri");
            }
        }
    }
}
