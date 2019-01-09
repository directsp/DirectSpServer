using System.Collections.Generic;
using System.Data;

namespace DirectSp.ProcedureInfos
{
    public class SpInfo
    {
        public string SchemaName { get; set; }
        public string ProcedureName { get; set; }
        public SpParamInfo[] Params { get; set; } = { };
        public SpInfoEx ExtendedProps { get; set; } = new SpInfoEx();
    }
}
