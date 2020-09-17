using DirectSp.Host.Settings;
using DirectSp.Providers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Host
{
    static class App
    {
        public static DirectSpInvoker DirectSpInvoker { get; set; }
        public static HostSettings HostSettings { get; set; }
        public static KestrelSettings KestrelSettings { get; set; }
        public static X509Certificate2 KestrelSslCertificate { get; set; }
        public static AuthProviderSettings[] AuthProviderSettings { get; set; }

        public static void Configure(IConfiguration configuration)
        {
            //load settings
            HostSettings = configuration.GetSection("DirectSpHost").Get<HostSettings>() ?? new HostSettings();
            KestrelSettings = configuration.GetSection("Kestrel").Get<KestrelSettings>() ?? new KestrelSettings();
            AuthProviderSettings = configuration.GetSection("AuthProviders").Get<AuthProviderSettings[]>() ?? new AuthProviderSettings[0];

            var directSpInvokerOptions = configuration.GetSection("DirectSpInvoker").Get<DirectSpInvokerOptions>();
            directSpInvokerOptions.WorkspaceFolderPath = Path.Combine(HostSettings.WorkspaceFolderPath, "DirectSp");
            Directory.CreateDirectory(HostSettings.WorkspaceFolderPath);

            // Create KeyValue instance base of AppSetting.json settings
            directSpInvokerOptions.KeyValueProvider = HostSettings.KeyValueProvider.Name switch
            {
                KeyValueProviderType.SqlKeyValue => new SqlKeyValueProvider(HostSettings.KeyValueProvider.ConnectionString),
                KeyValueProviderType.MemoryKeyValue => new MemoryKeyValueProvder(),
                _ => throw new NotImplementedException($"KeyValueProvider has not been implemented. Name: {HostSettings.KeyValueProvider.Name}"),
            };
            directSpInvokerOptions.CommandProvider = new SqlCommandProvider(HostSettings.ResourceDbConnectionString);
            DirectSpInvoker = new DirectSpInvoker(directSpInvokerOptions);

            // find Kestrel Ssl Certificate
            if (!string.IsNullOrEmpty(KestrelSettings.ListenIp) && !string.IsNullOrEmpty(KestrelSettings.SslCertificateThumb))
                KestrelSslCertificate = DirectSpInvoker.CertificateProvider.GetByThumb(KestrelSettings.SslCertificateThumb);
        }
    }
}
