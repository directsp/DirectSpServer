﻿using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using DirectSp.Core.InternalDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DirectSp.Core.Test.TestClass
{
    [TestClass]
    public class CaptchaHandlerTest
    {
        [TestMethod]
        public async Task CreateCaptchaTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            var captchaHandler = new CaptchaHandler(dspKeyValue);
            var captcha = await captchaHandler.Create();
            Assert.IsTrue(captcha.Image != null || captcha.Image.Length > 0);
            var keyValueItem = (DspKeyValueItem)await dspKeyValue.GetValue(captcha.Id);
            Assert.IsTrue(captcha.Id == keyValueItem.KeyName);
        }

        [TestMethod]
        public async Task VerifyCaptchaTest()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            var captchaHandler = new CaptchaHandler(dspKeyValue);
            var captcha = await captchaHandler.Create();

            // Get captcha code for verfing test
            var keyValueItem = (DspKeyValueItem)await dspKeyValue.GetValue(captcha.Id);
            var captchaCode = keyValueItem.TextValue;

            // Check invalid response for captcha
            try
            {
                var wrongCode = $"{captchaCode}Wrong";
                await captchaHandler.Verify(captcha.Id, wrongCode, "TestMethod");
            }
            catch (SpInvalidCaptchaException) { }

            // Check valid response for captcha
            captcha = await captchaHandler.Create();
            keyValueItem = (DspKeyValueItem)await dspKeyValue.GetValue(captcha.Id);
            captchaCode = keyValueItem.TextValue;
            await captchaHandler.Verify(captcha.Id, captchaCode, "TestMethod");

        }

        [TestMethod]
        public async Task RejectCaptchaInSecondChecking_FirstCheckingHasBeenSucceeded()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            var captchaHandler = new CaptchaHandler(dspKeyValue);
            var captcha = await captchaHandler.Create();

            // Get captcha code for verfing test
            var keyValueItem = (DspKeyValueItem)await dspKeyValue.GetValue(captcha.Id);
            var captchaCode = keyValueItem.TextValue;

            await captchaHandler.Verify(captcha.Id, captchaCode, "TestMethod");

            try
            {
                await captchaHandler.Verify(captcha.Id, captchaCode, "TestMethod");
            }
            catch (SpInvalidCaptchaException) { }
        }

        [TestMethod]
        public async Task RejectCaptchaInSecondChecking_FirstCheckingHasBeenFailed()
        {
            var dspKeyValue = new DspMemoryKeyValue();
            var captchaHandler = new CaptchaHandler(dspKeyValue);
            var captcha = await captchaHandler.Create();

            // Get captcha code for verfing test
            var keyValueItem = (DspKeyValueItem)await dspKeyValue.GetValue(captcha.Id);
            var captchaCode = keyValueItem.TextValue;

            try
            {
                var wrongCode = $"{captchaCode}Wrong";
                await captchaHandler.Verify(captcha.Id, wrongCode, "TestMethod");
            }
            catch (SpInvalidCaptchaException) { }

            try
            {
                await captchaHandler.Verify(captcha.Id, captchaCode, "TestMethod");
            }
            catch (SpInvalidCaptchaException) { }
        }
    }
}
