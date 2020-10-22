using System.Net;

namespace DirectSp.Host.Settings
{
    class KestrelSettings
    {
        public IPEndPoint EndPoint { get; set; }
        public string CertificateThumb { get; set; }
    }
}