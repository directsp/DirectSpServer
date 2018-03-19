using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSpLib.Entities
{
    public class SpInvokerOptions
    {
        public int SqlStoreProceduresUpdateInterval { get; set; } = 60; // 60 seconds
        public int SessionTimeout { get; set; } = 30 * 60; //30 minutes
        public int SessionMaxRequestCount { get; set; } = 100; //100 requests
        public int SessionMaxRequestCycleInterval { get; set; } = 5 * 60; //5 minutes
        public int ReadonlyConnectionSyncInterval { get; set; } = 10; //10 seconds
        public bool UseCamelCase { get; set; } = true;
        public int DownloadedRecordsetFileLifetime { get; set; } = 2 * 3600;
        public System.Globalization.CultureInfo AlternativeCalendar { get; set; }
    }
}
