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
        public static DirectSpInvoker DirectSpInvoker { get; private set; }
        public static HostSettings HostSettings { get; private set; } = new HostSettings();
        public static KestrelSettings KestrelSettings { get; private set; } = new KestrelSettings();
        public static X509Certificate2 KestrelSslCertificate { get; private set; }

        public static void Configure(IConfiguration configuration)
        {
            //load settings
            configuration.GetSection("DirectSpHost").Bind(HostSettings);
            configuration.GetSection("Kestrel").Bind(KestrelSettings);

            var directSpInvokerOptions = new DirectSpInvokerOptions() {
                WorkspaceFolderPath = Path.Combine(HostSettings.WorkspaceFolderPath, "DirectSp")
            };
            configuration.GetSection("DirectSpInvoker").Bind(directSpInvokerOptions);
            Directory.CreateDirectory(HostSettings.WorkspaceFolderPath);

            // Create KeyValue instance base of AppSetting.json settings
            switch (HostSettings.KeyValueProvider.Name)
            {
                case KeyValueProviderType.SqlKeyValue:
                    directSpInvokerOptions.KeyValueProvider = new SqlKeyValueProvider(HostSettings.KeyValueProvider.ConnectionString);
                    break;

                case KeyValueProviderType.MemoryKeyValue:
                    directSpInvokerOptions.KeyValueProvider = new MemoryKeyValueProvder();
                    break;

                default:
                    throw new NotImplementedException($"KeyValueProvider has not been implemented. Name: {HostSettings.KeyValueProvider.Name}");
            }

            directSpInvokerOptions.CommandProvider = new SqlCommandProvider(HostSettings.ResourceDbConnectionString);
            DirectSpInvoker = new DirectSpInvoker(directSpInvokerOptions);

            // find Kestrel Ssl Certificate
            if (!string.IsNullOrEmpty(KestrelSettings.ListenIp) && !string.IsNullOrEmpty(KestrelSettings.SslCertificateThumb))
                KestrelSslCertificate = DirectSpInvoker.CertificateProvider.GetByThumb(KestrelSettings.SslCertificateThumb);
        }
    }
}
