using DirectSpAuth.Settings;
using Microsoft.Extensions.Configuration;
using DirectSpLib;
using DirectSpLib.Entities;
using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSpAuth
{
    public static class App
    {
        public static AppSettings AppSettings = new AppSettings();
        public static KestlerSettings KestlerSettings = new KestlerSettings();

        public static SpInvoker SpInvoker { get; private set; }
        public static X509Certificate2 ServerCertificate { get; private set; }
        public static X509Certificate2 KestrelSslCertificate { get; private set; }

        public static void Configure(IConfigurationRoot configuration)
        {
            configuration.GetSection("App").Bind(AppSettings);
            configuration.GetSection("Kestrel").Bind(KestlerSettings);

            var spInvokerOptions = new SpInvokerOptions();
            configuration.GetSection("SpInvoker").Bind(spInvokerOptions);

            var spInvokerInternal = new SpInvoker(AppSettings.InternalDbConnectionString, AppSettings.InternalDbSchema, spInvokerOptions);
            SpInvoker = new SpInvoker(AppSettings.ResourceDbConnectionString, AppSettings.ResourceDbSchema, spInvokerOptions, spInvokerInternal);

            // Set server certificate
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.OpenExistingOnly);
            var certList = certStore.Certificates.Find(X509FindType.FindByThumbprint, AppSettings.ServerCertificateThumb, false);
            ServerCertificate = certList.Count > 0 ? certList[0] : null; 
            if (ServerCertificate == null)
                throw new Exception($"Could not find Server certificate: Thumb: {AppSettings.ServerCertificateThumb}!");

            // find Kestrel Ssl Certificate
            if (!string.IsNullOrEmpty(KestlerSettings.ListenIp) && !string.IsNullOrEmpty(KestlerSettings.SslCertificateThumb))
            {
                certList = certStore.Certificates.Find(X509FindType.FindByThumbprint, KestlerSettings.SslCertificateThumb, false);
                KestrelSslCertificate = certList.Count > 0 ? certList[0] : null;
            }
        }
    }
}
