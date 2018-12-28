using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Core
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetByThumb(string thumbNumber);
    }
}
