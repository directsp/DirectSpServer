namespace DirectSp.Core.Entities
{
    public class SpInvokerOptions
    {
        public int SessionTimeout { get; set; } = 30 * 60; //30 minutes
        public int SessionMaxRequestCount { get; set; } = 100; //100 requests
        public int SessionMaxRequestCycleInterval { get; set; } = 5 * 60; //5 minutes
        public int ReadonlyConnectionSyncInterval { get; set; } = 10; //10 seconds
        public bool UseCamelCase { get; set; } = true;
        public int DownloadedRecordsetFileLifetime { get; } = 5 * 3600; //5 hours
        public System.Globalization.CultureInfo AlternativeCalendar { get; set; }
        public string WorkspaceFolderPath { get; set; }
        public bool IsDownloadEnabled { get; set; } = true;
    }
}
