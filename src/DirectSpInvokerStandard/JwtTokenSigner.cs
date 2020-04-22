using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace DirectSp
{
    internal class JwtTokenSigner
    {
        public ICertificateProvider CertificateProvider { get; }
        public JwtTokenSigner(ICertificateProvider certificateProvider)
        {
            CertificateProvider = certificateProvider;
        }

        public string Sign(string jwt)
        {
            //  Find certificate by thumb number 
            var obj = JsonSerializer.Deserialize<JsonElement>(jwt);
            if (!obj.TryGetProperty("CertificateThumb", out JsonElement je))
                throw new Exception("CertificateThumb does not exist in jwt");
            
            var certificateThumb = je.GetString();
            if (string.IsNullOrEmpty(certificateThumb))
                throw new NullReferenceException(nameof(certificateThumb));

            RSA rsa = CertificateProvider.GetByThumb(certificateThumb).GetRSAPrivateKey();

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
            var obj = JsonSerializer.Deserialize<JsonElement>(payload);

            // Check token expiration
            var exp = Util.DateTime_FromUnixDate(obj.GetProperty("exp").GetDouble());
            if (DateTime.Now > exp)
                throw new ArgumentException("Token has been expired.", nameof(jwt));

            var rsa = (RSA)CertificateProvider.GetByThumb(obj.GetProperty("CertificateThumb").GetString()).PublicKey.Key;

            // Check sign by certificate public key
            var sha256 = SHA256.Create();
            var encoding = new UTF8Encoding();
            var data = encoding.GetBytes(payload);
            var hash = sha256.ComputeHash(data);

            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
