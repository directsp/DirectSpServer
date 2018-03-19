using Newtonsoft.Json;
using DirectSpLib.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectSpLib.InternalDb
{
    internal class Captcha
    {
        public SpInvoker SpInvokerInterval { get; private set; }

        public Captcha(SpInvoker spInvokerInternal)
        {
            SpInvokerInterval = spInvokerInternal;
        }

        private static byte[] CreateCaptchaImageByNumber(int number)
        {
            var humanReadableIntegerProvider = new DNTCaptcha.Core.Providers.HumanReadableIntegerProvider();
            var numberText = humanReadableIntegerProvider.NumberToText(number, DNTCaptcha.Core.Providers.Language.Persian);
            var randomNumberProvider = new DNTCaptcha.Core.Providers.RandomNumberProvider();
            var captchaImageProvider = new DNTCaptcha.Core.Providers.CaptchaImageProvider(randomNumberProvider);
            var captchaImage = captchaImageProvider.DrawCaptcha(numberText, "#00b6b5", "#eeeeee", 20, "Tahoma");
            return captchaImage;
        }

        public async Task<SpCallResult> Create()
        {
            var spCall = new SpCall() { Method = "Captcha_" + nameof(Create) };
            var captchaCode = new Random().Next(100, 999);
            var image = CreateCaptchaImageByNumber(captchaCode);
            spCall.Params.Add("Code", captchaCode);
            var ret = await SpInvokerInterval.Invoke(spCall);
            ret["Image"] = Convert.ToBase64String(image); //add captcha image to result
            return ret;
        }

        public async Task<SpCallResult> Match(string captchaId, string code)
        {
            var spCall = new SpCall() { Method = "Captcha_" + nameof(Match) };
            spCall.Params.Add("CaptchaId", captchaId);
            spCall.Params.Add("Code", code);
            var ret = await SpInvokerInterval.Invoke(spCall);
            return ret;
        }
    }
}