using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.Client
{
    public class SpCall
    {
        public string method { get; set; }

        [JsonProperty("params")]
        public object param { get; set; } = new object();
    }
}
