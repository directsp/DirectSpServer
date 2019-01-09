using DirectSp.Entities;
using DirectSp.Host.Settings;
using DirectSp.Providers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Host
{
    static class App
    {
        public static Invoker Invoker { get; private set; }
        public static HostSettings HostSettings { get; private set; } = new HostSettings();
        public static KestlerSettings KestlerSettings { get; private set; } = new KestlerSettings();
        public static X509Certificate2 KestrelSslCertificate { get; private set; }

        public static void Configure(IConfigurationRoot configuration)
        {
            //load settings
            configuration.GetSection("DirectSpHost").Bind(HostSettings);
            configuration.GetSection("Kestler").Bind(KestlerSettings);

            var invokerOptions = new InvokerOptions() {
                WorkspaceFolderPath = Path.Combine(HostSettings.WorkspaceFolderPath, "DirectSp")
            };
            configuration.GetSection("DirectSpInvoker").Bind(invokerOptions);
            Directory.CreateDirectory(HostSettings.WorkspaceFolderPath);

            // Create KeyValue instance base of AppSetting.json settings
            switch (HostSettings.KeyValueProvider.Name)
            {
                case KeyValueProviderType.SqlKeyValue:
                    invokerOptions.KeyValueProvider = new SqlKeyValueProvider(HostSettings.KeyValueProvider.ConnectionString);
                    break;

                case KeyValueProviderType.MemoryKeyValue:
                    invokerOptions.KeyValueProvider = new MemoryKeyValueProvder();
                    break;

                default:
                    throw new NotImplementedException($"KeyValueProvider has not been implemented. Name: {HostSettings.KeyValueProvider.Name}");
            }

            Invoker = new Invoker(invokerOptions);

            // find Kestrel Ssl Certificate
            if (!string.IsNullOrEmpty(KestlerSettings.ListenIp) && !string.IsNullOrEmpty(KestlerSettings.SslCertificateThumb))
            {
                //Invoker.CertificateProvider.GetByThumb(); todo
                //var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.OpenExistingOnly);
                //var certList = certStore.Certificates.Find(X509FindType.FindByThumbprint, KestlerSettings.SslCertificateThumb, true);
                //KestrelSslCertificate = certList.Count > 0 ? certList[0] : null;
                KestrelSslCertificate = Invoker.CertificateProvider.GetByThumb(KestlerSettings.SslCertificateThumb);
            }
        }
    }
}
