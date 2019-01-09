using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DirectSp.Core.Infrastructure
{
    public interface ICertificateProvider
    {
        X509Certificate2 GetByThumb(string thumbNumber);
    }
}
