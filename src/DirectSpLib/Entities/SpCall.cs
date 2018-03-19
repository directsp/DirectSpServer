using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSpLib.Entities
{
    public class SpCall
    {
        public string Method { get; set; }
        public IDictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
    }
}
