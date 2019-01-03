using DirectSp.Core.ProcedureInfos;
using System;

namespace DirectSp.Core
{
   [AttributeUsage(AttributeTargets.Method)]
    public class DirectSpProcAttribute : Attribute
    {
        public bool IsBatchAllowed { get; set; }
    }
}
