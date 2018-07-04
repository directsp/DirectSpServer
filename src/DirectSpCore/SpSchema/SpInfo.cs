using System.Collections.Generic;
using System.Data;

namespace DirectSp.Core.SpSchema
{
    public class SpInfo
    {
        public string SchemaName { get; set; }
        public string ProcedureName { get; set; }
        public SpParam[] Params { get; set; } = { };
        public SpInfoEx ExtendedProps { get; set; } = new SpInfoEx();
    }
}
