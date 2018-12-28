using DirectSp.Core.Entities;

namespace DirectSp.Core.Exceptions
{
    public class SpInvalidCaptchaException : SpException
    {
        internal SpInvalidCaptchaException(CaptchaRequest newCaptcha, SpException baseException) : base(baseException)
        {
            _SpCallError.ErrorData = newCaptcha;
        }

        internal SpInvalidCaptchaException(CaptchaRequest newCaptcha, string spName)
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
