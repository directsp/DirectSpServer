using System.Collections.Generic;
using System.Data;

namespace DirectSp.Core.Entities
{
    public enum SpExecuteMode
    {
        NotSet,
        Write,
        ReadSync,
        ReadSnapshot,
        ReadWise,
    };

    public enum SpCaptchaMode
    {
        Manual,
        Always,
        Auto,
    };


    public class SpParamEx
    {
        public bool IsUseMoneyConversionRate { get; set; }
    }

    public class SpFieldEx
    {
        public bool IsUseMoneyConversionRate { get; set; }
    }

    public class SpInfoEx
    {
        public SpExecuteMode ExecuteMode { get; set; } = SpExecuteMode.NotSet;
        public bool IsBatchAllowed { get; set; }
        public SpCaptchaMode CaptchaMode { get; set; } = SpCaptchaMode.Manual;
        public IDictionary<string, SpParamEx> Params { get; set; } = new Dictionary<string, SpParamEx>();
        public IDictionary<string, SpFieldEx> Fields { get; set; } = new Dictionary<string, SpFieldEx>();
        public int CommandTimeout { get; set; } = 30;
    }

    public class SpParam
    {
        public string ParamName { get; set; }
        public SqlDbType SystemTypeName { get; set; }
        public string UserTypeName { get; set; }
        public int Length { get; set; }
        public bool IsOutput { get; set; }
    }

    public class SpInfo
    {
        public string SchemaName { get; set; }
        public string ProcedureName { get; set; }
        public SpParam[] Params { get; set; } = { };
        public SpInfoEx ExtendedProps { get; set; } = new SpInfoEx();
    }
}
