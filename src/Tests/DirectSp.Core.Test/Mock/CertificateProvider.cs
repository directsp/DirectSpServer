using DirectSp.Core.Infrastructure;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Core.Test.Mock
{
    public class CertificateProvider : ICertificateProvider
    {
        public X509Certificate2 GetByThumb(string thumbNumber)
        {
            // Get certificates physical path
             string certificatePath = "../../../certificates/star.certificate.test.pfx";

            // Load certificate by physical path
            X509Certificate2 certificate = new X509Certificate2(certificatePath,"1");

            return (X509Certificate2)certificate;
        }
    }
}
