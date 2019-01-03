using DirectSp.Core.Providers;
using DirectSp.Core.Entities;
using DirectSp.Core.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using DirectSp.Core.Exceptions;
using System;

namespace DirectSp.Core.Test.TestClass
{
    [TestClass]
    public class SpInvokerTest
    {
        private Invoker _invoker;

        [TestInitialize]
        public void Init()
        {

            // Resolve SpInvoker internal dependencies
            var invokerOptions = new InvokerOptions
            {
                WorkspaceFolderPath = "Workspace/Directsp",
                Schema = "TestObject",
                CommandProvider = new ObjectCommandProvider(new TestObject()),
                CertificateProvider = new MockCertificateProvider(),
                Logger = Logger.Current

            };

            _invoker = new Invoker(invokerOptions);
        }

        [TestMethod]
        public async Task TestSimpleInvoke()
        {
            var spCall = new SpCall
            {
                Method = "Test1",
                Params = Util.Dyn2Dict(new { param2="v2", param3 = "v3" } )
            };

            var result = await _invoker.Invoke(spCall);
            Assert.IsTrue((string)result.ReturnValue == "Test1_result");
            Assert.IsTrue((string)result["param1"] == "v2_v3_v4");
            Assert.IsTrue((string)result["param2"] == "param2_result");
        }

        [TestMethod]
        public async Task TestParallelInvokeSp()
        {
            var spCalls = new SpCall[10];

            for (int i = 0; i < spCalls.Length; i++)
            {
                spCalls[i] = new SpCall
                {
                    Method = "Test2_Batch",
                    Params = Util.Dyn2Dict(new { param2 = "v2", param3 = "v3" })
                };
            }

            var invokeOptions = new SpInvokeParams()
            {
                AuthUserId = "1",
                UserRemoteIp = "127.0.0.1"
            };

            var result = await _invoker.Invoke(spCalls, invokeOptions);

            Assert.AreEqual(10, result.Length);
        }


        [TestMethod]
        public async Task TestJwtTokenSign()
        {
            var spCall = new SpCall
            {
                Method = "Test3_SignParam",
            };

            var result = await _invoker.Invoke(spCall);
            Assert.IsTrue((string)result["jwtToken"] == Data.JwtToken_signed);
        }

        [TestMethod]
        public async Task TestJwtTokenValidate_CheckPass()
        {
            var spCall = new SpCall
            {
                Method = "Test4_ValidateSign",
                Params = Util.Dyn2Dict(new { jwtToken = Data.JwtToken_signed  })
            };

            await _invoker.Invoke(spCall);
        }

        [TestMethod]
        public async Task TestJwtTokenValidate_CheckReject()
        {
            var spCall = new SpCall
            {
                Method = "Test4_ValidateSign",
                Params = Util.Dyn2Dict(new { jwtToken = Data.JwtToken_unsigned  })
            };

            try
            {
                await _invoker.Invoke(spCall);

            }
            catch (SpInvalidParamSignature)
            {
                //it is expected
            }
        }

        [TestMethod]
        public async Task TestDouplicateRequestHandling()
        {
            var spCall = new SpCall
            {
                Method = "TestSimple"
            };

            var spInvokeParams = new SpInvokeParams { InvokeOptions = new InvokeOptions { RequestId = Guid.NewGuid().ToString() }, UserRemoteIp = "1" };
            await _invoker.Invoke(spCall, spInvokeParams);

            try
            {
                await _invoker.Invoke(spCall, spInvokeParams);
                Assert.Fail("SpDuplicateRequestException was expected!");
            }
            catch (SpDuplicateRequestException)
            {
            }
        }
    }
}
