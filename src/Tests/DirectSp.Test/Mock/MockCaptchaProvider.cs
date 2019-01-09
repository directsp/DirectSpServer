namespace DirectSp.Test.Mock
{
    internal class MockCaptchaProvider : ICaptchaProvider
    {
        public Captcha Generate()
        {
            return new Captcha() { Text = "123", ImageBuffer = new byte[10] };
        }
    }
}
