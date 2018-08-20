using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Entities
{
    public class DspKeyValueItem
    {
        public string KeyName { get; set; }
        public string TextValue { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
