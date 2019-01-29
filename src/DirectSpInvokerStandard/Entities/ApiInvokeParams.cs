using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp
{
    public class ApiInvokeParams
    {
        public SpCall SpCall { get; set; }
        public ApiInvokeOptions InvokeOptions { get; set; } = new ApiInvokeOptions();
    }
}
