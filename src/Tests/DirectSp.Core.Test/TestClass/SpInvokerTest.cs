using DirectSp.Core.DI;
using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.Test.DI;
using DirectSp.Core.Test.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSp.Core.Test
{
    [TestClass]
    public class SpInvokerTest
    {
        private SpInvoker _spInvoker;

        [TestInitialize]
        public void Init()
        {

            SpInvokerOptions spInvokerOptions = new SpInvokerOptions();

            // Create SpInvoke instance base on application settings
            _spInvoker = new SpInvoker("", "api", spInvokerOptions);

            // Swich resolver ninject module to TestModule
            Resolver.Instance.SetModule(new TestModule());
        }

        [TestMethod]
        public async Task TestParallelInvokeSp()
        {
            var spCalls = new SpCall[10];

            for (int i = 0; i < 10; i++)
            {
                spCalls[i] = new SpCall
                {
                    Method = "ParallelSp",
                    Params = new Dictionary<string, object>
                    {
                        {"Param1","" }
                    }
                };
            }

            var invokeOptions = new SpInvokeParams()
            {
                AuthUserId = "1",
                UserRemoteIp = "127.0.0.1"
            };

            var result = await _spInvoker.Invoke(spCalls, invokeOptions);

            Assert.AreEqual(10, result.Length);

        }

        [TestMethod]
        public async Task TestJwtTokenSign()
        {

            var spCall = new SpCall
            {
                Method = "SignJwtToken",
                Params = new Dictionary<string, object>
                {
                    {"JwtToken","" }
                }
            };

            var result = await _spInvoker.Invoke(spCall);
            Assert.IsTrue((string)result["JwtToken"] == Data.SignedJwtToken());
        }

        [TestMethod]
        public async Task TestJwtTokenSign_CheckValid()
        {
            var spCall = new SpCall
            {
                Method = "SignJwtTokenChecking",
                Params = new Dictionary<string, object>
                {
                    {"JwtToken", Data.SignedJwtToken() }
                }
            };

            await _spInvoker.Invoke(spCall);
        }

        [TestMethod]
        public async Task TestJwtTokenSign_CheckInvalid()
        {
            var spCall = new SpCall
            {
                Method = "SignJwtTokenChecking",
                Params = new Dictionary<string, object>
                {
                    {"JwtToken", "kfdlklsdkf.lkdlklsdkf.lkddflksdlfkd" }
                }
            };

            try
            {
                await _spInvoker.Invoke(spCall);
                Assert.Fail();
            }
            catch (Exception) { }
        }

    }
}
