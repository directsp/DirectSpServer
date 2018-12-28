using System.Data;

namespace DirectSp.Core.ProcedureInfos
{
    public class SpParam
    {
        public string ParamName { get; set; }
        public SqlDbType SystemTypeName { get; set; }
        public string UserTypeName { get; set; }
        public int Length { get; set; }
        public bool IsOutput { get; set; }
    }
}
