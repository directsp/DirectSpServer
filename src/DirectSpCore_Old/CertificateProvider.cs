using DirectSp.Core.Infrastructure;
using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Core
{
    public class CertificateProvider : ICertificateProvider
    {
        public X509Certificate2 GetByThumb(string thumbNumber)
        {
            X509Store myStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            myStore.Open(OpenFlags.ReadOnly);

            // Finding certificate by thumb number
            var certificates = myStore.Certificates.Find(X509FindType.FindByThumbprint, thumbNumber.ToUpper(), false);

            // Throw null reference exception if certificate is not exist
            if (certificates == null || certificates.Count == 0)
                throw new NullReferenceException("Certificate's not found!!!");

            return certificates[0];
        }
    }
}
