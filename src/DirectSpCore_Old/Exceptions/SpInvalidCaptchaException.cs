using Newtonsoft.Json.Linq;
using DirectSp.Core.Entities;
using DirectSp.Core.InternalDb;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Exceptions
{
    public class SpInvalidCaptchaException : SpException
    {
        internal SpInvalidCaptchaException(Captcha newCaptcha, SpException baseException) : base(baseException)
        {
            _SpCallError.ErrorData = newCaptcha;
        }

        internal SpInvalidCaptchaException(Captcha newCaptcha, string spName)
            : base(new SpCallError()
            {
                ErrorName = SpCommonExceptionId.InvalidCaptcha.ToString(),
                ErrorNumber = (int)SpCommonExceptionId.InvalidCaptcha,
                ErrorMessage = $"Invalid captcha for invoking {spName}",
                ErrorData = newCaptcha

            })
        { }

    }
}
