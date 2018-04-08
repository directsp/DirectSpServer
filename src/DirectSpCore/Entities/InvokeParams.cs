using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Entities
{
    public class InvokeParams
    {
        public SpCall SpCall { get; set; }
        public InvokeOptions InvokeOptions { get; set; } = new InvokeOptions();
    }
}
