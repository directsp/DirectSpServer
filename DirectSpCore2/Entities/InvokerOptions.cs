using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DirectSp.Core.Entities
{
    public class InvokerOptions
    {
        public string Schema { get; set; } = "api";
        public int SessionTimeout { get; set; } = 30 * 60; //30 minutes
        public int SessionMaxRequestCount { get; set; } = 100; //100 requests
        public int SessionMaxRequestCycleInterval { get; set; } = 5 * 60; //5 minutes
        public int ReadonlyConnectionSyncInterval { get; set; } = 10; //10 seconds
        public bool UseCamelCase { get; set; } = true;
        public int DownloadedRecordsetFileLifetime { get; } = 5 * 3600; //5 hours
        public CultureInfo AlternativeCulture { get; set; }
        public string WorkspaceFolderPath { get; set; }
        public bool IsDownloadEnabled { get; set; } = true;
        public IKeyValueProvider KeyValueProvider { get; set; }
        public ICommandProvider CommandProvider { get; set; }
        public ICertificateProvider CertificateProvider { get; set; }
        public ICaptchaProvider CaptchaProvider { get; set; }
        public ILogger Logger { get; set; }
    }
}
