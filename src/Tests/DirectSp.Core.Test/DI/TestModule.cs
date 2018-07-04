using DirectSp.Core.Infrastructure;
using Ninject.Modules;
using System.Data;

namespace DirectSp.Core.Test.DI
{
    class TestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICertificateProvider>().To<Mock.CertificateProvider>();
            Bind<ICommandExecuter>().To<Mock.CommandExecuter>().InSingletonScope();
            Bind<JwtTokenSigner>().ToSelf();
        }
    }
}
