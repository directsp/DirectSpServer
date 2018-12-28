using Microsoft.Extensions.Configuration;
using DirectSp.Core;
using DirectSp.Core.Entities;
using System;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using DirectSp.Core.Database;
using DirectSp.AuthServer.Settings;
using DirectSp.Core.InternalDb;

namespace DirectSp.AuthServer
{
    static class App
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

            Directory.CreateDirectory(AppSettings.WorkspaceFolderPath);
            var spInvokerOptions = new SpInvokerOptions() { WorkspaceFolderPath = Path.Combine(AppSettings.WorkspaceFolderPath, "DirectSp") };
            configuration.GetSection("SpInvoker").Bind(spInvokerOptions);


            // Resolve SpInvoker internal dependencies
            var spInvokerConfig = new SpInvokerConfig
            {
                ConnectionString = AppSettings.ResourceDbConnectionString,
                Options = spInvokerOptions,
                Schema = AppSettings.ResourceDbSchema,
                KeyValue = null,
                TokenSigner = new JwtTokenSigner(new CertificateProvider()),
                DbLayer = new DbLayer()
            };

            // Create KeyValue instance base of AppSetting.json settings
            switch (AppSettings.KeyValueProvider.Name)
            {
                case KeyValueProviderType.DspSqlKeyValue:
                    spInvokerConfig.KeyValue = new DspSqlKeyValue(AppSettings.KeyValueProvider.ConnectionString);
                    break;
                case KeyValueProviderType.DspMemoryKeyValue:
                    spInvokerConfig.KeyValue = new MemoryKeyValue();
                    break;
                default:
                    throw new NotImplementedException($"KeyValueProvider has not been implemented. Name: {AppSettings.KeyValueProvider.Name}");
            }

            SpInvoker = new SpInvoker(spInvokerConfig);

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
