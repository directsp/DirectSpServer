using DirectSp.Core.Providers;
using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System.Linq;
using System.ComponentModel;

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
                Logger = Logger.Current

            };

            _invoker = new Invoker(invokerOptions);
        }

        private Dictionary<string, object> Dyn2Dict(object dynObj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
            {
                object obj = propertyDescriptor.GetValue(dynObj);
                dictionary.Add(propertyDescriptor.Name, obj);
            }
            return dictionary;
        }

        [TestMethod]
        public async Task SimpleInvoke()
        {
            var spCall = new SpCall
            {
                Method = "Test1",
                Params = Dyn2Dict(new { param2="v2", param3 = "v3" } )
            };

            var result = await _invoker.Invoke(spCall);
            Assert.IsTrue((string)result.ReturnValue == "Test1_result");
            Assert.IsTrue((string)result["param1"] == "v2_v3_v4");
            Assert.IsTrue((string)result["param2"] == "param2_result");
        }

        //    [TestMethod]
        //    public async Task TestParallelInvokeSp()
        //    {
        //        var spCalls = new SpCall[10];

        //        for (int i = 0; i < 10; i++)
        //        {
        //            spCalls[i] = new SpCall
        //            {
        //                Method = "ParallelSp",
        //                Params = new Dictionary<string, object>
        //                {
        //                    {"Param1","" }
        //                }
        //            };
        //        }

        //        var invokeOptions = new SpInvokeParams()
        //        {
        //            AuthUserId = "1",
        //            UserRemoteIp = "127.0.0.1"
        //        };

        //        var result = await _invoker.Invoke(spCalls, invokeOptions);

        //        Assert.AreEqual(10, result.Length);

        //    }

        //    [TestMethod]
        //    public async Task TestJwtTokenSign()
        //    {

        //        var spCall = new SpCall
        //        {
        //            Method = "SignJwtToken",
        //            Params = new Dictionary<string, object>
        //                {
        //                // Value of this parameter is setting by DbLayer
        //                    {"JwtToken","" }
        //                }
        //        };

        //        var result = await _invoker.Invoke(spCall);
        //        Assert.IsTrue((string)result["JwtToken"] == Data.SignedJwtToken());
        //    }

        //    [TestMethod]
        //    public async Task TestJwtTokenSign_CheckValid()
        //    {
        //        var spCall = new SpCall
        //        {
        //            Method = "SignJwtTokenChecking",
        //            Params = new Dictionary<string, object>
        //                {
        //                    {"JwtToken", Data.SignedJwtToken() }
        //                }
        //        };

        //        await _invoker.Invoke(spCall);
        //    }

        //    [TestMethod]
        //    public async Task TestJwtTokenSign_CheckInvalid()
        //    {
        //        var spCall = new SpCall
        //        {
        //            Method = "SignJwtTokenChecking",
        //            Params = new Dictionary<string, object>
        //            {
        //                {"JwtToken", "kfdlklsdkf.lkdlklsdkf.lkddflksdlfkd" }
        //            }
        //        };

        //        try
        //        {
        //            await _invoker.Invoke(spCall);
        //            Assert.Fail();
        //        }
        //        catch (Exception) { }
        //    }

        //    [TestMethod]
        //    public async Task TestDouplicateRequestHandling()
        //    {
        //        var spCall = new SpCall
        //        {
        //            Method = "TestApi"
        //        };

        //        var requestId = Guid.NewGuid();
        //        var spInvokeParams = new SpInvokeParams { InvokeOptions = new InvokeOptions { RequestId = requestId.ToString() }, UserRemoteIp = "1" };
        //        await _invoker.Invoke(spCall, spInvokeParams);

        //        try
        //        {
        //            await _invoker.Invoke(spCall, spInvokeParams);
        //        }
        //        catch (SpDuplicateRequestException)
        //        {
        //            Assert.IsTrue(true);
        //        }
        //    }

        //    [TestMethod]
        //    public Task TestBulkErrorWrapper()
        //    {
        //        throw new NotImplementedException();
        //    }

        //}
    }
}
