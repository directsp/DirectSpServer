using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSpClientNet
{
    internal class InvokeParams
    {
        public SpCall spCall { get; set; }
        public InvokeOptions invokeOptions { get; set; } = new InvokeOptions();
    }
}
