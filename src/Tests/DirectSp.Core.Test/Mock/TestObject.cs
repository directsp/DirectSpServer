using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Core.Test.Mock
{
    public class TestObject
    {
        public string Test1(out string param1, ref string param2, string param3, string param4 = "v4")
        {
            param1 = param2 + "_" + param3 + "_" + param4;
            param2 = "param2_result"; 
            return "Test1_result";
        }
    }
}
