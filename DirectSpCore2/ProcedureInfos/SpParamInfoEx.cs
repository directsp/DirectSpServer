
namespace DirectSp.Core.ProcedureInfos
{
    public enum SpSignType
    {
        None = 0,
        JwtByCertThumb = 1,
    }

    public class SpParamInfoEx
    {
        public SpSignType SignType { get; set; }
    }
}
