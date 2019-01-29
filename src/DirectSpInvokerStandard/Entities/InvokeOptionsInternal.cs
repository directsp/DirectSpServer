using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp
{
    class InvokeOptionsInternal
    {
        public InvokeOptions SpInvokeParams = new InvokeOptions();
        public bool IsCaptcha { get; set; }
        public bool IsBatch { get; set; }
        public bool IsSystem { get; set; }
        public bool IsForceReadOnly { get; set; }
    }
}
