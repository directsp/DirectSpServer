using Microsoft.IdentityModel.Tokens;

namespace DirectSp.Host
{
    public class AuthSettings
    {
        public string CertificateFile { get; set; }
        public string ValidIssuer { get; set; }
        public string[] ValidAudiences { get; set; }
        public int ClockSkew { get; set; } = (int)TokenValidationParameters.DefaultClockSkew.TotalSeconds;
    }

    public class AppSettings
    {
        public string ResourceDbConnectionString { get; set; }
        public string ResourceDbSchema { get; set; } = "api";
        public string InternalDbConnectionString { get; set; }
        public string InternalDbSchema { get; set; } = "api";
        public AuthSettings Authentication { get; set; }
        public bool EnableCors { get; set; }
        public int InvokeDelayInterval { get; set; }
    }
}
