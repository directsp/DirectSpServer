using DirectSp.Core.ProcedureInfos;

namespace DirectSp.Core.Test.Mock
{
    public class TestObject
    {
        public int TestSimple()
        {
            return 1;
        }

        public string Test1(out string param1, ref string param2, string param3, string param4 = "v4")
        {
            param1 = param2 + "_" + param3 + "_" + param4;
            param2 = "param2_result"; 
            return "Test1_result";
        }

        [DirectSpProc(IsBatchAllowed = true)]
        public string Test2_Batch(out string param1, ref string param2, string param3, string param4 = "v4")
        {
            param1 = param2 + "_" + param3 + "_" + param4;
            param2 = "param2_result";
            return "Test1_Batch_result";
        }

        public void Test3_SignParam([DirectSpParam(SignType = SpSignType.JwtByCertThumb)] out string jwtToken)
        {
            jwtToken = Data.JwtToken;
        }

        public void Test4_ValidateSign([DirectSpParam(SignType = SpSignType.JwtByCertThumb)] string jwtToken)
        {
        }


    }
}
