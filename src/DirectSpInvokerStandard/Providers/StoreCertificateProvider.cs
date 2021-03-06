﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Providers
{
    public class StoreCertificateProvider : ICertificateProvider
    {
        public X509Certificate2 GetByThumb(string thumbNumber)
        {
            using (var myStore = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                myStore.Open(OpenFlags.ReadOnly);

                // Finding certificate by thumb number
                var certificates = myStore.Certificates.Find(X509FindType.FindByThumbprint, thumbNumber.ToUpper(), false);

                // Throw null reference exception if certificate is not exist
                if (certificates == null || certificates.Count == 0)
                    throw new Exception("Could not find the certificate!");

                return certificates[0];
            }
        }
    }
}
