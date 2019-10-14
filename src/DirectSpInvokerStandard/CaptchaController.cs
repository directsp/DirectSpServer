using System;
using System.Threading.Tasks;
using DirectSp.Exceptions;

namespace DirectSp
{
    internal class CaptchaController
    {
        private readonly IKeyValueProvider _KeyValueProvider;
        public ICaptchaProvider CaptchaProvider { get; }

        public CaptchaController(IKeyValueProvider keyValueProvider, ICaptchaProvider  captchaProvider)
        {
            _KeyValueProvider = keyValueProvider;
            CaptchaProvider = captchaProvider;
        }

        public async Task<CaptchaRequest> Create()
        {
            var captchaData = CaptchaProvider.Generate();
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
                    throw new InvalidCaptchaException(reCreate ? await Create() : null, procName);
            }
            catch (SpAccessDeniedOrObjectNotExistsException)
            {
                throw new InvalidCaptchaException(reCreate ? await Create() : null, procName);
            }
        }
    }
}