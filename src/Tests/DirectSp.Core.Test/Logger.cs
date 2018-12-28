using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectSp.Core.Test
{
    static class Logger
    {
        private static Lazy<ILogger> _current = new Lazy<ILogger>(() => Init());
        public static ILogger Current => _current.Value;

        private static ILogger Init()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging( builder => builder
                .AddConsole()
            );

            var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Console");
            return logger;
        }

    }
}
