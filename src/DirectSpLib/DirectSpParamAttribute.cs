using DirectSp.ProcedureInfos;
using System;

namespace DirectSp
{
   [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class DirectSpParamAttribute : Attribute
    {
        public SpSignType SignType { get; set; } = SpSignType.None;
    }
}
