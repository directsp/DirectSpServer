using DirectSp.Core;
using DirectSp.Core.Database;
using DirectSp.Core.Entities;
using DirectSp.Core.InternalDb;
using DirectSp.Host.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Host
{
    static class App
    {
        public static SpInvoker SpInvoker { get; private set; }
        public static AppSettings AppSettings { get; private set; } = new AppSettings();
        public static KestlerSettings KestlerSettings = new KestlerSettings();
        public static X509Certificate2 KestrelSslCertificate { get; private set; }

        public static void Configure(IConfigurationRoot configuration)
        {
            //load settings
            configuration.GetSection("App").Bind((object)AppSettings);
            configuration.GetSection("Kestrel").Bind(KestlerSettings);

            Directory.CreateDirectory((string)AppSettings.WorkspaceFolderPath);
            var spInvokerOptions = new SpInvokerOptions() { WorkspaceFolderPath = Path.Combine((string)AppSettings.WorkspaceFolderPath, "DirectSp") };
            configuration.GetSection("SpInvoker").Bind(spInvokerOptions);

            // Resolve SpInvoker internal dependencies
            var spInvokerConfig = new SpInvokerConfig
            {
                DownloadEnable = AppSettings.DownloadEnable,
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
                    spInvokerConfig.KeyValue = new DspMemoryKeyValue();
                    break;

                default:
                    throw new NotImplementedException($"KeyValueProvider has not been implemented. Name: {AppSettings.KeyValueProvider.Name}");
            }

            SpInvoker = new SpInvoker(spInvokerConfig);

            // find Kestrel Ssl Certificate
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine, OpenFlags.OpenExistingOnly);
            if (!string.IsNullOrEmpty(KestlerSettings.ListenIp) && !string.IsNullOrEmpty(KestlerSettings.SslCertificateThumb))
            {
                var certList = certStore.Certificates.Find(X509FindType.FindByThumbprint, KestlerSettings.SslCertificateThumb, true);
                KestrelSslCertificate = certList.Count > 0 ? certList[0] : null;
            }
        }
    }
}
