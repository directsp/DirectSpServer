using DirectSp.Core.Database;
using DirectSp.Core.Infrastructure;
using Ninject.Modules;
using System.Data;
using System.Data.SqlClient;

namespace DirectSp.Core.DI
{
    class CoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICertificateProvider>().To<CertificateProvider>();
            Bind<IDbLayer>().To<DbLayer>().InSingletonScope();
            Bind<JwtTokenSigner>().ToSelf();
        }
    }
}
