using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp
{
    public class InvokeOptions
    {
        public bool IsLocalRequest { get; set; } = true;
        public string RequestRemoteIp { get; set; }
        public string AuthUserId { get; set; }
        public string Audience { get; set; }
        public ApiInvokeOptions ApiInvokeOptions { get; set; } = new ApiInvokeOptions();
        public string RecordsetDownloadUrlTemplate { get; set; } //should contain {id} and {filename}
        public string UserAgent { get; set; }
    }
}
