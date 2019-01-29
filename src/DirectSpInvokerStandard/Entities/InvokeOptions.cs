using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp
{
    public class InvokeOptions
    {
        public string UserRemoteIp { get; set; }
        public string AuthUserId { get; set; }
        public string Audience { get; set; }
        public ApiInvokeOptions ApiInvokeOptions { get; set; } = new ApiInvokeOptions();
        public string RecordsetDownloadUrlTemplate { get; set; } //should contain {id} and {filename}
    }
}
