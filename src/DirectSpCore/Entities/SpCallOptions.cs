using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DirectSp.Core.Entities
{
    public class SpCallOptions
    {
        [DefaultValue(false)]
        public bool IsBatch { get; set; } = false;

        [DefaultValue(false)]
        public bool IsCaptcha { get; set; } = false;

        [DefaultValue(1.0f)]
        public float? MoneyConversionRate { get; set; } = 1.0f;

        [DefaultValue(null)]
        public int? RecordIndex { get; set; }

        [DefaultValue(null)]
        public int? RecordCount { get; set; }

        [DefaultValue(null)]
        public string ClientVersion { get; set; }

        [DefaultValue(null)]
        public string InvokerAppVersion { get; set; }

    }
}
