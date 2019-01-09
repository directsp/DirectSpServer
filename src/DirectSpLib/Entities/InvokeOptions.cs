namespace DirectSp.Entities
{
    public class InvokeOptions
    {
        public string RequestId { get; set; }
        public string CaptchaId { get; set; }
        public string CaptchaCode { get; set; }
        public RecordsetFormat RecordsetFormat { get; set; } = RecordsetFormat.Json;
        public string RecordsetFileTitle { get; set; }
        public bool IsWithRecodsetDownloadUri { get; set; }
        public int? RecordIndex { get; set; }
        public int? RecordCount { get; set; }
    };

}
