using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSpLib.Entities
{
    public class SpCallOptions
    {
        public bool IsBatch { get; set; } = false;
        public bool IsCaptcha { get; set; } = false;
        public float MoneyConversionRate { get; set; } = 1;
        public int? RecordIndex { get; set; }
        public int? RecordCount { get; set; }
    }
}
