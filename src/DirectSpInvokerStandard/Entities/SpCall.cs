using System.Collections.Generic;

namespace DirectSp
{
    public class SpCall
    {
        public string Method { get; set; }
        public IDictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
    }
}
