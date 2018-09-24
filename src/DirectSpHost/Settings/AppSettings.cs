using System.IO;

namespace DirectSp.Host.Settings
{
    class AppSettings
    {
        public bool DownloadEnable { get; set; } = true;
        public string ResourceDbConnectionString { get; set; }
        public string ResourceDbSchema { get; set; } = "api";
        public AuthSettings Authentication { get; set; } = new AuthSettings();
        public KeyValueProviderSettings KeyValueProvider { get; set; } = new KeyValueProviderSettings();
        public bool EnableCors { get; set; }
        public int InvokeDelayInterval { get; set; }
        public string WorkspaceFolderPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "Workspace");
    }
}
