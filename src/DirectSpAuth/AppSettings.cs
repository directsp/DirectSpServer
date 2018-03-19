using Microsoft.IdentityModel.Tokens;
using System;

namespace DirectSpAuth.Settings
{
    public class AppSettings
    {
        public string ResourceDbConnectionString { get; set; }
        public string ResourceDbSchema { get; set; } = "api";
        public string InternalDbConnectionString { get; set; }
        public string InternalDbSchema { get; set; } = "api";
        public string ServerCertificateThumb { get; set; }
        public string ServerIssuerUri { get; set; }
        public string PersistKeysFolderPath { get; set; }
        public string ServerAudience { get; set; }
        public string SignInUri { get; set; }
        public string SignInClientId { get; set; }
        public string MyAccountClientId { get; set; }
        public int InvokeDelayInterval { get; set; }
        public bool AllowInsecureHttp { get; set; }
        public int AccessTokenLifetime { get; set; } = 1800;// 30 min
        public int RefreshTokenLifetime { get; set; } = 3600 * 24 * 90; //90 days
        public int ClockSkew { get; set; } = (int)TokenValidationParameters.DefaultClockSkew.TotalSeconds;
    }
}