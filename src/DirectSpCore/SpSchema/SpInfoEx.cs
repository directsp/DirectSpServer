﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.SpSchema
{
    public class SpInfoEx
    {
        public SpExecuteMode ExecuteMode { get; set; } = SpExecuteMode.NotSet;
        public bool IsBatchAllowed { get; set; }
        public SpCaptchaMode CaptchaMode { get; set; } = SpCaptchaMode.Manual;
        public IDictionary<string, SpParamEx> Params { get; set; } = new Dictionary<string, SpParamEx>();
        public IDictionary<string, SpFieldEx> Fields { get; set; } = new Dictionary<string, SpFieldEx>();
        public int CommandTimeout { get; set; } = 30;
    }
}