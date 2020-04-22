using System;
using Microsoft.Extensions.Logging;

namespace DirectSp.Test
{
    static class Log
    {
        private static readonly Lazy<ILogger> _current = new Lazy<ILogger>(() => Init());
        public static ILogger Current => _current.Value;

        private static ILogger Init()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            return new Logger<object>(loggerFactory);
        }

    }
}
