using System.Security.Cryptography.X509Certificates;

namespace DirectSp
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetByThumb(string thumbNumber);
    }
}
