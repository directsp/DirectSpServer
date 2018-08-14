using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DirectSp.Core
{
    public static class Logger
    {
        private static Lazy<ILog> log4Net = new Lazy<ILog>(() => Start());
        public static ILog Log4Net => log4Net.Value;
        public static string LogFolder { get; set; }

        private static ILog Start()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            return LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType
               );
        }
    }
}
