using Microsoft.IdentityModel.Tokens;

namespace DirectSp.Host.Settings
{
   class AuthSettings
    {
        public string CertificateFile { get; set; }
        public string ValidIssuer { get; set; }
        public string[] ValidAudiences { get; set; }
        public int ClockSkew { get; set; } = (int)TokenValidationParameters.DefaultClockSkew.TotalSeconds;
    }
}
