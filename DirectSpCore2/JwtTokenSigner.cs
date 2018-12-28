using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DirectSp.Core
{
    internal class JwtTokenSigner
    {
        private ICertificateProvider _certificateProvider;
        public JwtTokenSigner(ICertificateProvider certificateProvider)
        {
            _certificateProvider = certificateProvider;
        }

        public string Sign(string jwt)
        {
            //  Find certificate by thumb number 
            var obj = JObject.Parse(jwt);
            var certificateThumb = (string)obj["CertificateThumb"];
            if (string.IsNullOrEmpty(certificateThumb))
                throw new NullReferenceException(nameof(certificateThumb));

            RSA rsa = _certificateProvider.GetByThumb(certificateThumb).GetRSAPrivateKey();

            if (rsa == null)
                throw new Exception("Could not found any valid certificate!");

            // Sign token using certificate private key
            var encoding = new UTF8Encoding();
            var sha256 = SHA256.Create();
            var data = encoding.GetBytes(jwt);
            var hash = sha256.ComputeHash(data);
            var signBase64 = Convert.ToBase64String(rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            var tokenHeader = @"{'alg': 'SHA256', 'typ': 'JWT'}".Replace("'", "\"");
            return $"{StringHelper.ToBase64(tokenHeader)}.{StringHelper.ToBase64(jwt)}.{signBase64}";
        }

        public bool CheckSign(string jwt)
        {
            var jwtParts = jwt.Split('.');
            if (jwtParts == null || jwtParts.Length < 3)
                throw new ArgumentException("Token does not have 3 part!", nameof(jwt));

            var signature = Convert.FromBase64String(jwtParts[2]);

            //  Find certificate by thumb number 
            var payload = StringHelper.FromBase64(jwtParts[1]);
            var obj = JObject.Parse(payload);

            // Check token expiration
            var exp = Util.DateTime_FromUnixDate((double)obj["exp"]);
            if (DateTime.Now > exp)
                throw new ArgumentException("Token has been expired.", nameof(jwt));

            var rsa = (RSA)_certificateProvider.GetByThumb((string)obj["CertificateThumb"]).PublicKey.Key;

            // Check sign by certificate public key
            var sha256 = SHA256.Create();
            var encoding = new UTF8Encoding();
            var data = encoding.GetBytes(payload);
            var hash = sha256.ComputeHash(data);

            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
