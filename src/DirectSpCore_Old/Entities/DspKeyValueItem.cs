using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Entities
{
    public class KeyValueItem
    {
        public string KeyName { get; set; }
        public string TextValue { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
