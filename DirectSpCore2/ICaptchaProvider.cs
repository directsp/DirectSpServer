using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core
{
    public class Captcha
    {
        public string Text;
        public byte[] ImageBuffer;
    }

    public interface ICaptchaProvider
    {
        Captcha Generate();
    }
}
