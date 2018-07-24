using DirectSp.Core.Helpers;
using DirectSp.Core.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            RSA rsa = _certificateProvider.GetByThumb(certificateThumb).GetRSAPrivateKey();

            if (rsa == null)
                throw new Exception("No valid cert was found");

            // Sign token using certificate private key
            UTF8Encoding encoding = new UTF8Encoding();
            SHA256 sha256 = SHA256.Create();
            byte[] data = encoding.GetBytes(jwt);
            byte[] hash = sha256.ComputeHash(data);
            string sign = Convert.ToBase64String(rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            string tokenHeader = @"{'alg': 'SHA256', 'typ': 'JWT'}".Replace("'", "\"");
            return $"{tokenHeader.ToBase64()}.{jwt.ToBase64()}.{sign}";
        }

        public bool CheckSign(string jwt)
        {
            var jwtParts = jwt.Split('.');
            if (jwtParts == null || jwtParts.Length < 3)
                throw new ArgumentException("Token does not have 3 part!" ,nameof(jwt));

            var signature = Convert.FromBase64String(jwtParts[2]);

            //  Find certificate by thumb number 
            var payload = jwtParts[1].FromBase64();
            dynamic json = JObject.Parse(payload);
            RSA rsa = _certificateProvider.GetByThumb(json.CertificateThumb.ToString()).PublicKey.Key;

            // Check sign by certificate public key
            SHA256 sha256 = SHA256.Create();
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(payload);
            byte[] hash = sha256.ComputeHash(data);

            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
