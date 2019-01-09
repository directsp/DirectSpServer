using DirectSp.ProcedureInfos;
using System;

namespace DirectSp
{
   [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DirectSpProcAttribute : Attribute
    {
        public bool IsBatchAllowed { get; set; }
        public SpCaptchaMode CaptchaMode { get; set; } = SpCaptchaMode.Manual;
    }
}
