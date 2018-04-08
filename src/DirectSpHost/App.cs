using System;
using DirectSp.Core;
using Microsoft.Extensions.Configuration;
using DirectSp.Core.InternalDb;
using System.Security.Cryptography.X509Certificates;
using DirectSp.Core.Entities;

namespace DirectSp.Host
{
    public static class App
    {
        public static SpInvoker SpInvoker { get; private set; }
        public static AppSettings AppSettings { get; private set; } = new AppSettings();
        public static KestlerSettings KestlerSettings = new KestlerSettings();
        public static X509Certificate2 KestrelSslCertificate { get; private set; }

        public static void Configure(IConfigurationRoot configuration)
        {
            //load settings
            var spInvokerOptions = new SpInvokerOptions();
            configuration.GetSection("App").Bind(AppSettings);
            configuration.GetSection("SpInvoker").Bind(spInvokerOptions);
            configuration.GetSection("Kestrel").Bind(KestlerSettings);

            //load SpInvokers Settings
            var apiInvokerInternal = new SpInvoker(AppSettings.InternalDbConnectionString, AppSettings.InternalDbSchema, spInvokerOptions);
            SpInvoker = new SpInvoker(AppSettings.ResourceDbConnectionString, AppSettings.ResourceDbSchema, spInvokerOptions, apiInvokerInternal);

            // find Kestrel Ssl Certificate
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.OpenExistingOnly);
            if (!string.IsNullOrEmpty(KestlerSettings.ListenIp) && !string.IsNullOrEmpty(KestlerSettings.SslCertificateThumb))
            {
                var certList = certStore.Certificates.Find(X509FindType.FindByThumbprint, KestlerSettings.SslCertificateThumb, false);
                KestrelSslCertificate = certList.Count > 0 ? certList[0] : null;
            }
        }
    }
}
