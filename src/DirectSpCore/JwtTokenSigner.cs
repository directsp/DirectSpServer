using DirectSp.Core.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DirectSp.Core
{
    public class JwtTokenSigner
    {
        private ICertificateProvider _certificateProvider;
        public JwtTokenSigner(ICertificateProvider certificateProvider)
        {
            _certificateProvider = certificateProvider;
        }

        public string Sign(string jwt)
        {
            //  Find certificate by thumb number 
            dynamic json = JObject.Parse(jwt);
            string certificateThumb = json.CertificateThumb;
            if (string.IsNullOrEmpty(certificateThumb))
                throw new NullReferenceException(nameof(certificateThumb));

            RSA rsa = _certificateProvider.GetByThumb(certificateThumb).GetRSAPrivateKey();

            if (rsa == null)
                throw new Exception("Could not found any valid certificate!");

            // Sign token using certificate private key
            UTF8Encoding encoding = new UTF8Encoding();
            SHA256 sha256 = SHA256.Create();
            byte[] data = encoding.GetBytes(jwt);
            byte[] hash = sha256.ComputeHash(data);
            string sign = Convert.ToBase64String(rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            string tokenHeader = @"{'alg': 'SHA256', 'typ': 'JWT'}".Replace("'", "\"");
            return $"{StringHelper.ToBase64(tokenHeader)}.{StringHelper.ToBase64(jwt)}.{sign}";
        }

        public bool CheckSign(string jwt)
        {
            var jwtParts = jwt.Split('.');
            if (jwtParts == null || jwtParts.Length < 3)
                throw new ArgumentException("Token does not have 3 part!", nameof(jwt));

            var signature = Convert.FromBase64String(jwtParts[2]);

            //  Find certificate by thumb number 
            var payload = StringHelper.FromBase64(jwtParts[1]);
            dynamic json = JObject.Parse(payload);

            // Check token expiration
            var exp = Util.DateTime_FromUnixDate((double)json.exp);
            if (DateTime.Now > exp)
                throw new ArgumentException("Token has been expired.", nameof(jwt));

            RSA rsa = _certificateProvider.GetByThumb(json["CertificateThumb"].ToString()).PublicKey.Key;

            // Check sign by certificate public key
            SHA256 sha256 = SHA256.Create();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(payload);
            byte[] hash = sha256.ComputeHash(data);

            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
