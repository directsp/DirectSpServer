using DirectSp.Providers;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DirectSp
{
    public class DirectSpInvokerOptions
    {
        /// <summary>
        /// If set then AppVersion of API will be ignored
        /// </summary>
        public string AppVersion { get; set; }
        public int SessionTimeout { get; set; } = 30 * 60; //30 minutes
        public int SessionMaxRequestCount { get; set; } = 100; //100 requests
        public int SessionMaxRequestCycleInterval { get; set; } = 5 * 60; //5 minutes
        public int ReadonlyConnectionSyncInterval { get; set; } = 10; //10 seconds
        public bool UseCamelCase { get; set; } = true;
        public int DownloadedRecordsetFileLifetime { get; set; } = 5 * 3600; //5 hours
        public CultureInfo AlternativeCulture { get; set; }
        public string WorkspaceFolderPath { get; set; }
        public bool IsDownloadEnabled { get; set; } = true;
        public ICommandProvider CommandProvider { get; set; }
        public ICaptchaProvider CaptchaProvider { get; set; }
        public IKeyValueProvider KeyValueProvider { get; set; } = new MemoryKeyValueProvder();
        public ICertificateProvider CertificateProvider { get; set; } = new StoreCertificateProvider();
        public ILogger Logger { get; set; }
    }
}
