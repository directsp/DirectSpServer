using DirectSp.Core.Entities;
using System;
using System.Threading.Tasks;
using DirectSp.Core.Exceptions;
using Newtonsoft.Json;

namespace DirectSp.Core
{
    internal class CaptchaRequest
    {
        [JsonProperty("CaptchaId")]
        public string Id { get; set; }

        [JsonProperty("CaptchaImage")]
        public byte[] ImageBuffer { get; set; }
    }

    internal class CaptchaController
    {
        private readonly IKeyValueProvider _KeyValueProvider;
        private readonly ICaptchaProvider _CaptchaProvider;

        public CaptchaController(IKeyValueProvider keyValueProvider, ICaptchaProvider  captchaProvider)
        {
            _KeyValueProvider = keyValueProvider;
            _CaptchaProvider = captchaProvider;
        }

        public async Task<CaptchaRequest> Create()
        {
            var captchaData = _CaptchaProvider.Generate();
            var captchaId = $"captcha/{Guid.NewGuid()}";
            await _KeyValueProvider.SetValue(captchaId, captchaData.Text, 600, true);
            return new CaptchaRequest
            {
                Id = captchaId,
                ImageBuffer = captchaData.ImageBuffer
            };
        }

        public async Task Verify(string captchaId, string text, string procName, bool reCreate = true)
        {
            try
            {
                var captcha = (KeyValueItem)await _KeyValueProvider.GetValue(captchaId);
                await _KeyValueProvider.Delete(captchaId);

                if (!captcha.TextValue.Equals(text, StringComparison.OrdinalIgnoreCase))
                    throw new SpInvalidCaptchaException(reCreate ? await Create() : null, procName);
            }
            catch (SpAccessDeniedOrObjectNotExistsException)
            {
                throw new SpInvalidCaptchaException(reCreate ? await Create() : null, procName);
            }

        }



    }
}