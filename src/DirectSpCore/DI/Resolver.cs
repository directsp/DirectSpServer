using Ninject;
using Ninject.Modules;
using System;

namespace DirectSp.Core.DI
{
    public class Resolver
    {
        // Singltone pattern
        private static Lazy<Resolver> _instance = new Lazy<Resolver>(() => new Resolver());
        public static Resolver Instance => _instance.Value;

        private IKernel _kernel;

        public void SetModule(NinjectModule value)
        {
            _kernel = new StandardKernel(value);
        }

        private Resolver()
        {
            _kernel = new StandardKernel(new CoreModule());
        }

        public T Resolve<T>()
        {
            return _kernel.Get<T>();
        }
    }
}
