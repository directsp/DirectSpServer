using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DirectSp.Core.Infrastructure;
using DirectSp.Core.Exceptions;
using System.Data.SqlClient;

namespace DirectSp.Core.InternalDb
{
    public class CaptchaHandler
    {
        private IDspKeyValue _dspKeyValue;

        public CaptchaHandler(IDspKeyValue dspKeyValue)
        {
            _dspKeyValue = dspKeyValue;
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

        public async Task<Captcha> Create()
        {
            var captchaCode = new Random().Next(100, 999);
            var image = CreateCaptchaImageByNumber(captchaCode);
            var captchaId = $"captcha/{Guid.NewGuid()}";
            await _dspKeyValue.SetValue(captchaId, captchaCode.ToString(), 600, true);
            return new Captcha
            {
                Id = captchaId,
                Image = image
            };
        }

        public async Task Verify(string captchaId, string code, string spName, bool reCreate = true)
        {
            try
            {
                var captcha = (KeyValueItem)await _dspKeyValue.GetValue(captchaId);
                await _dspKeyValue.Delete(captchaId);

                if (!captcha.TextValue.Equals(code, StringComparison.OrdinalIgnoreCase))
                    throw new SpInvalidCaptchaException(reCreate ? await Create() : null, spName);
            }
            catch (SpAccessDeniedOrObjectNotExistsException)
            {
                throw new SpInvalidCaptchaException(reCreate ? await Create() : null, spName);
            }

        }



    }
}