using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Entities
{
    class SpInvokeParamsInternal
    {
        public SpInvokeParams SpInvokeParams = new SpInvokeParams();
        public bool IsCaptcha { get; set; }
        public bool IsBatch { get; set; }
        public bool IsSystem { get; set; }
        public bool IsForceReadOnly { get; set; }
    }
}
