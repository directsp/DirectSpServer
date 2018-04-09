using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.Client
{
    internal class InvokeParamsBatch
    {
        public SpCall[] spCalls { get; set; }
        public InvokeOptions invokeOptions { get; set; } = new InvokeOptions();
    }
}
