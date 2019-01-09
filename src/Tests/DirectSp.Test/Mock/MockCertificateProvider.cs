using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Test.Mock
{
    class MockCertificateProvider : ICertificateProvider
    {
        public X509Certificate2 GetByThumb(string thumbNumber)
        {
            // Get certificates physical path
            var certificatePath = "../../../certificates/star.certificate.test.pfx";

            // Load certificate by physical path
            var certificate = new X509Certificate2(certificatePath, "1");

            return certificate;
        }
    }
}
