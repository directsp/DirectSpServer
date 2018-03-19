using Newtonsoft.Json.Linq;
using DirectSpLib.Entities;
using DirectSpLib.InternalDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSpLib.Exceptions
{
    public class SpInvalidCaptchaException : SpException
    {
        internal SpInvalidCaptchaException(SpInvoker spInvokerInternal, SpException baseException) : base(baseException) {
            AttachNewCaptcha(spInvokerInternal);
        }

        internal SpInvalidCaptchaException(SpInvoker spInvokerInternal, string spName)
            : base(new SpCallError() { ErrorName= SpCommonExceptionId.InvalidCaptcha.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidCaptcha, ErrorMessage = $"Invalid captcha for invoking {spName}" })
        {
            AttachNewCaptcha(spInvokerInternal);
        }

        public void AttachNewCaptcha(SpInvoker spInvokerInternal)
        {
            //create new captcha for error
            var captcha = new Captcha(spInvokerInternal);
            var spCallResult = captcha.Create().Result;
            var captchaData = new JObject
            {
                ["CaptchaId"] = spCallResult["CaptchaId"].ToString(),
                ["CaptchaImage"] = spCallResult["Image"].ToString()
            };
            _SpCallError.ErrorData = captchaData;
        }
    }
}
