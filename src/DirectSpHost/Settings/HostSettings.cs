namespace DirectSp.Host.Settings
{
    internal class HostSettings
    {
        public string WorkspaceFolderPath { get; set; }
        public string ResourceDbConnectionString { get; set; }
        public AuthSettings Authentication { get; set; } = new AuthSettings();
        public KeyValueProviderSettings KeyValueProvider { get; set; } = new KeyValueProviderSettings();
        public bool EnableCors { get; set; }
        public int InvokeDelayInterval { get; set; }
    }
}
