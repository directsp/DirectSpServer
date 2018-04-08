﻿namespace DirectSp.DirectSpClient
{
    public class InvokeOptions
    {
        public string captchaId { get; set; }
        public string captchaCode { get; set; }
        public bool isWithRecordsetFields { get; set; }
        public RecordsetFormat recordsetFormat { get; set; } = RecordsetFormat.json;
        public string recordsetFileTitle { get; set; }
        public bool isWithRecodsetDownloadUri { get; set; }
        public float moneyConversionRate { get; set; } = 1;
        public int? recordIndex { get; set; }
        public int? recordCount { get; set; }
        public bool IsAntiXss { get; set; } = true;
    };

}
