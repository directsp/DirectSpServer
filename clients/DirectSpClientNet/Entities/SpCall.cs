using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSpClientNet
{
    internal class SpCall
    {
        public string method { get; set; }

        [JsonProperty("params")]
        public object args { get; set; } = new object();
    }
}
