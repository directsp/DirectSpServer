namespace DirectSp.Host.Settings
{
    class HostSettings
    {
        public string WorkspaceFolderPath { get; set; }
        public string ResourceDbConnectionString { get; set; }
        public AuthProviderSettings Authentication { get; set; }
        public KeyValueProviderSettings KeyValueProvider { get; set; } = new KeyValueProviderSettings();
        public bool EnableCors { get; set; }
        public int InvokeDelayInterval { get; set; }
    }
}
