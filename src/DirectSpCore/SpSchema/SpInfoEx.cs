using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.ProcedureInfos
{
    public class SpInfoEx
    {
        public SpDataAccessMode DataAccessMode { get; set; } = SpDataAccessMode.Write;
        public bool IsBatchAllowed { get; set; }
        public SpCaptchaMode CaptchaMode { get; set; } = SpCaptchaMode.Manual;
        public IDictionary<string, SpParamEx> Params { get; set; } = new Dictionary<string, SpParamEx>();
        public IDictionary<string, SpFieldEx> Fields { get; set; } = new Dictionary<string, SpFieldEx>();
        public int CommandTimeout { get; set; } = 30;
    }
}
