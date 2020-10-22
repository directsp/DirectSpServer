using DirectSp.Host.Settings;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Xml;

namespace DirectSp.Host.Auth
{
    public class AppSecurityTokenValidator : JwtSecurityTokenHandler
    {
        private readonly string _signatureValidatorUrl;

        public AppSecurityTokenValidator(string signatureValidatorUrl)
        {
            _signatureValidatorUrl = signatureValidatorUrl;
        }

        protected override JwtSecurityToken ValidateSignature(string token, TokenValidationParameters validationParameters)
        {
            var httpClient = new HttpClient();
            if (!httpClient.GetAsync(string.Format(_signatureValidatorUrl, token)).Result.IsSuccessStatusCode)
                throw new SecurityTokenInvalidSignatureException();

           return ReadJwtToken(token);
        }
    }
}
