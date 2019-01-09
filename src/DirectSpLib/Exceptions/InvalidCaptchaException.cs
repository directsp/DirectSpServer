using DirectSp.Entities;

namespace DirectSp.Exceptions
{
    public class InvalidCaptchaException : DirectSpException
    {
        internal InvalidCaptchaException(CaptchaRequest newCaptcha, DirectSpException baseException) : base(baseException)
        {
            _SpCallError.ErrorData = newCaptcha;
        }

        internal InvalidCaptchaException(CaptchaRequest newCaptcha, string spName)
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
