using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace DirectSp.Host
{
    public static class Logger
    {
        private static Lazy<ILog> log4Net = new Lazy<ILog>(() => Start());
        public static ILog Current => log4Net.Value;
        public static string LogFolder { get; set; }

        private static ILog Start()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            GlobalContext.Properties["tab"] = "\t";

            return LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );
        }
    }
}
