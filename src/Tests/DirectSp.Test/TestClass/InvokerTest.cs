using DirectSp.Exceptions;
using DirectSp.Providers;
using DirectSp.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSp.Test.TestClass
{
    [TestClass]
    public class SpInvokerTest
    {
        private DirectSpInvoker _directSpInvoker;
        private static readonly string JwtToken_unsigned = "eyJhbGciOiAiU0hBMjU2IiwgInR5cCI6ICJKV1QifQ==.eyJPcmRlck51bWJlciI6MiwiUmVjZWl2ZXJMb3lhbHR5QWNjb3VudElkIjoxMTAzNzkzMSwiQ2x1Yk5hbWUiOiJOaWtlIiwiQW1vdW50IjoxLCJQb2ludFR5cGVJZCI6MjAyMCwiUG9pbnRUeXBlTmFtZSI6IlRlc3RUeXBlIiwiUGF5ZWVMb3lhbHR5QWNjb3VudElkIjoxMTgzLCJQYXllZUxveWFsdHlBY2NvdW50TmFtZSI6IkJlaG5hbSBFeXZhenBvb3IiLCJleHAiOjI3OTcxMzI3MTUuMTI4NDQyMywiUmV0dXJuVXJsIjoiaHR0cDovL3d3dy5nb29nbGUuY29tIiwiQ2VydGlmaWNhdGVUaHVtYiI6Ijc3IDVhIDkyIDkyIGYxIDlmIDE0IGZjIDJiIDliIDgwIGFiIDA2IDJhIDA2IGJlIDg2IDIzIDYxIDljIn0=.6fHosNT0dNDqMo+wp2OdsdEPAOjlDU14HxngVcda60OmdxFz7II=";
        private static readonly string JwtToken_signed = "eyJhbGciOiAiU0hBMjU2IiwgInR5cCI6ICJKV1QifQ==.eyJPcmRlck51bWJlciI6MiwiUmVjZWl2ZXJMb3lhbHR5QWNjb3VudElkIjoxMTAzNzkzMSwiQ2x1Yk5hbWUiOiJOaWtlIiwiQW1vdW50IjoxLCJQb2ludFR5cGVJZCI6MjAyMCwiUG9pbnRUeXBlTmFtZSI6IlRlc3RUeXBlIiwiUGF5ZWVMb3lhbHR5QWNjb3VudElkIjoxMTgzLCJQYXllZUxveWFsdHlBY2NvdW50TmFtZSI6IkJlaG5hbSBFeXZhenBvb3IiLCJleHAiOjI3OTcxMzI3MTUuMTI4NDQyMywiUmV0dXJuVXJsIjoiaHR0cDovL3d3dy5nb29nbGUuY29tIiwiQ2VydGlmaWNhdGVUaHVtYiI6Ijc3IDVhIDkyIDkyIGYxIDlmIDE0IGZjIDJiIDliIDgwIGFiIDA2IDJhIDA2IGJlIDg2IDIzIDYxIDljIn0=.ELVhB5/a5rz0jI2WdIwnrzlOgm8s6eHz0yaCCAff1osfF4dhUWxUcDYVTBWadkHWelIh52qUsP0FVEV1075phsQDPuOPT7RR4BuP72nJzt/PsUoMb6fuKEygdutv3dyKEllZp7VAJny3PeSLf20aOy0MCXzdBDw7ZVF4kz/e62iwFHHqLwLDH1cfXaCAnRdEqtR6tkXwOYbvS1XJVw2fxVBBx1LLDLWD5q8gAtlVIGymI85AuveA477fcb0HzEz5ds9f3Wd0NkkGyolRSNcPlV6MHL/D2c6iF+nx6LDU9HTQ6jKPsKdjnbHRDwDo5Q1NeB8Z4FXHWutDpncRc+yCMA==";
        private static readonly string JwtToken = @"{'OrderNumber':2,'ReceiverLoyaltyAccountId':11037931,'ClubName':'Nike','Amount':1,'PointTypeId':2020,'PointTypeName':'TestType','PayeeLoyaltyAccountId':1183,'PayeeLoyaltyAccountName':'Behnam Eyvazpoor','exp':2797132715.1284423,'ReturnUrl':'http://www.google.com','CertificateThumb':'77 5a 92 92 f1 9f 14 fc 2b 9b 80 ab 06 2a 06 be 86 23 61 9c'}".Replace("'", "\"");


        [TestInitialize]
        public void Init()
        {

            // Resolve SpInvoker internal dependencies
            var invokerOptions = new DirectSpInvokerOptions
            {
                WorkspaceFolderPath = "Workspace/Directsp",
                Schema = "TestObject",
                CommandProvider = new ObjectCommandProvider(new TestObject()),
                CertificateProvider = new MockCertificateProvider(),
                CaptchaProvider = new MockCaptchaProvider(),
                Logger = Logger.Current

            };

            _directSpInvoker = new DirectSpInvoker(invokerOptions);
        }

        [TestMethod]
        public async Task TestSimpleInvoke()
        {
            //test1
            var result = await _directSpInvoker.Invoke("Test1", new { param2 = "v2", param3 = "v3" });
            Assert.AreEqual(result.ReturnValue, "Test1_result");
            Assert.AreEqual(result["param1"], "v2_v3_v4");
            Assert.AreEqual(result["param2"], "param2_result");
        }

        [TestMethod]
        public async Task TestContext()
        {
            //test1
            await _directSpInvoker.Invoke("TestContext", new { param1 = "a1" });

            var invokeOptions = new InvokeOptions()
            {
                ApiInvokeOptions = new ApiInvokeOptions()
                {
                    RecordCount = 215
                }
            };
            await _directSpInvoker.Invoke("TestContext", new { param1 = "a2" }, invokeOptions, true);
        }


        [TestMethod]
        public async Task TestSimpleInvoke_ChangeType()
        {
            //Test_Long
            var result = await _directSpInvoker.Invoke("Test_Long", new { param1 = (long)100 });
            Assert.AreEqual(result["param2"], 100);
        }

        [TestMethod]
        public async Task TestSimpleInvoke_Nullable()
        {
            //Test_Long
            var result = await _directSpInvoker.Invoke("Test_Nullable", new { param1 = (long?)100 });
            Assert.AreEqual(result["param2"], (long?)100);

            //Test_Long (null)
            result = await _directSpInvoker.Invoke("Test_Nullable", new { param1 = (long?)null });
            Assert.IsNull(result["param2"]);
        }


        [TestMethod]
        public async Task TestGetSetProperty()
        {
            var expectedValue = 2;

            //set property
            var result = await _directSpInvoker.Invoke("set_Prop1", new { value = expectedValue });

            //get property
            result = await _directSpInvoker.Invoke("get_Prop1", new { });
            Assert.AreEqual(result["returnValue"], expectedValue);
        }

        [TestMethod]
        public async Task TestException()
        {
            var expectedMessage = "FooException";
            try
            {
                await _directSpInvoker.Invoke("ThrowException", new { message = expectedMessage });
                Assert.Fail("Exception was expected");
            }
            catch (DirectSpException ex)
            {
                Assert.AreEqual(ex.SpCallError.ErrorMessage, expectedMessage);
            }
        }


        [TestMethod]
        public async Task TestParallelInvoke()
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

            var invokeOptions = new InvokeOptions()
            {
                AuthUserId = "1",
                RequestRemoteIp = "127.0.0.1"
            };

            var result = await _directSpInvoker.Invoke(spCalls, invokeOptions);

            Assert.AreEqual(10, result.Length);
        }


        [TestMethod]
        public async Task TestJwtTokenSign()
        {
            var result = await _directSpInvoker.Invoke("Test3_SignParam", new { json = JwtToken });
            Assert.IsTrue((string)result["jwtToken"] == JwtToken_signed);
        }

        [TestMethod]
        public async Task TestJwtTokenValidate_CheckPass()
        {
            await _directSpInvoker.Invoke("Test4_ValidateSign", new { jwtToken = JwtToken_signed });
        }

        [TestMethod]
        public async Task TestJwtTokenValidate_CheckReject()
        {
            try
            {
                await _directSpInvoker.Invoke("Test4_ValidateSign", new { jwtToken = JwtToken_unsigned });

            }
            catch (SpInvalidParamSignature)
            {
                //it is expected
            }
        }

        [TestMethod]
        public async Task TestDuplicateRequestException()
        {
            var spCall = new SpCall
            {
                Method = "TestSimple"
            };

            var spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { RequestId = Guid.NewGuid().ToString() }, RequestRemoteIp = "127.0.0.1" };
            await _directSpInvoker.Invoke(spCall, spInvokeParams);

            try
            {
                await _directSpInvoker.Invoke(spCall, spInvokeParams);
                Assert.Fail("SpDuplicateRequestException was expected!");
            }
            catch (DuplicateRequestException)
            {
            }
        }

        [TestMethod]
        public async Task TestCaptchaRequired()
        {
            var spCall = new SpCall
            {
                Method = "CaptchaRequiredMethod"
            };

            var spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { }, RequestRemoteIp = "127.0.0.1" };
            try
            {
                await _directSpInvoker.Invoke(spCall, spInvokeParams);
                Assert.Fail("SpInvalidCaptchaException was expected!");
            }
            catch (InvalidCaptchaException ex)
            {
                var captchaRequest = (CaptchaRequest)ex.SpCallError.ErrorData;

                //try with captcha
                try
                {
                    spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { CaptchaId = captchaRequest.Id, CaptchaCode = "1234" }, RequestRemoteIp = "127.0.0.1" };
                    await _directSpInvoker.Invoke(spCall, spInvokeParams);
                    Assert.Fail("SpInvalidCaptchaException was expected!");
                }
                catch (InvalidCaptchaException ex2)
                {
                    captchaRequest = (CaptchaRequest)ex2.SpCallError.ErrorData;
                    spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { CaptchaId = captchaRequest.Id, CaptchaCode = "123" }, RequestRemoteIp = "127.0.0.1" };
                    await _directSpInvoker.Invoke(spCall, spInvokeParams);

                    //second call with the same captcha id must fail
                    try
                    {
                        await _directSpInvoker.Invoke(spCall, spInvokeParams);
                        Assert.Fail("SpInvalidCaptchaException was expected for second retry!");
                    }
                    catch (InvalidCaptchaException)
                    {
                        //expected exception
                    }
                }
            }
        }

        [TestMethod]
        public async Task TestAsyncInt()
        {
            var spCall = new SpCall
            {
                Method = "AsyncIntMethod"
            };

            var spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { }, RequestRemoteIp = "127.0.0.1" };
            var result = await _directSpInvoker.Invoke(spCall, spInvokeParams);
            Assert.AreEqual(result.ReturnValue, 1);
        }

        [TestMethod]
        public async Task TestAsyncVoid()
        {
            var spCall = new SpCall
            {
                Method = "AsyncVoidMethod"
            };

            var spInvokeParams = new InvokeOptions { ApiInvokeOptions = new ApiInvokeOptions { }, RequestRemoteIp = "127.0.0.1" };
            var result = await _directSpInvoker.Invoke(spCall, spInvokeParams);

            try
            {
                var returnValue = result.ReturnValue;
                Assert.Fail("KeyNotFoundException expected!");
            }
            catch (KeyNotFoundException)
            {
                //expected
            }

        }
    }
}
