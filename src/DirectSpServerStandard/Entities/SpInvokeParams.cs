using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Entities
{
    public class SpInvokeParams
    {
        public string UserRemoteIp { get; set; }
        public string AuthUserId { get; set; }
        public string Audience { get; set; }
        public InvokeOptions InvokeOptions { get; set; } = new InvokeOptions();
        public string RecordsetDownloadUrlTemplate { get; set; } //should contain {id} and {filename}
    }
}
