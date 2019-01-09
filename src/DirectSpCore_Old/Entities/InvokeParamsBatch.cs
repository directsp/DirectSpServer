using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Entities
{
    public class InvokeParamsBatch
    {
        public SpCall[] SpCalls { get; set; }
        public InvokeOptions InvokeOptions { get; set; }
    }
}
