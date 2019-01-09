using DirectSp.ProcedureInfos;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSp.Test.Mock
{
    public class TestObject
    {
        public int TestSimple()
        {
            return 1;
        }

        public int Prop1 { get; set; }

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

        public void Test3_SignParam(string json, [DirectSpParam(SignType = SpSignType.JwtByCertThumb)] out string jwtToken)
        {
            jwtToken = json;
        }

        public void Test4_ValidateSign([DirectSpParam(SignType = SpSignType.JwtByCertThumb)] string jwtToken)
        {
        }

        [DirectSpProc(CaptchaMode =SpCaptchaMode.Always)]
        public void CaptchaRequiredMethod()
        {
        }

        public Task AsyncVoidMethod()
        {
            return Task.Factory.StartNew(() => Thread.Sleep(1));
        }

        public Task<int> AsyncIntMethod()
        {
            return Task.Factory.StartNew(() => { return 1; });
        }

        public void ThrowException(string message)
        {
            throw new Exception(message);
        }

    }
}
