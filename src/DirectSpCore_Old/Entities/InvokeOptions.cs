namespace DirectSp.Core.Entities
{
    public class InvokeOptions
    {
        public string RequestId { get; set; }
        public string CaptchaId { get; set; }
        public string CaptchaCode { get; set; }
        public bool IsWithRecordsetFields { get; set; }
        public RecordsetFormat RecordsetFormat { get; set; } = RecordsetFormat.Json;
        public string RecordsetFileTitle { get; set; }
        public bool IsWithRecodsetDownloadUri { get; set; }
        public float MoneyConversionRate { get; set; } = 1;
        public int? RecordIndex { get; set; }
        public int? RecordCount { get; set; }
    };

}
