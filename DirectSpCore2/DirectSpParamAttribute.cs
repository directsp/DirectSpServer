using DirectSp.Core.ProcedureInfos;
using System;

namespace DirectSp.Core
{
   [AttributeUsage(AttributeTargets.Parameter)]
    public class DirectSpParamAttribute : Attribute
    {
        public SpSignType SignType { get; set; } = SpSignType.None;
    }
}
