using Microsoft.IdentityModel.Tokens;

namespace DirectSp.Host.Settings
{
   public class AuthProviderSettings
    {
        public string Name { get; set; }
        public string[] Issuers { get; set; }
        public string[] ValidAudiences { get; set; }
        public string SignatureValidatorUrl { get; set; }
        public string NameClaimType { get; set; }
    }
}
