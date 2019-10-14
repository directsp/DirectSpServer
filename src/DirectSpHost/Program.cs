using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net;

namespace DirectSp.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return 
                WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    if (!string.IsNullOrEmpty(App.KestrelSettings.ListenIp))
                    {
                        options.Listen(IPAddress.Parse(App.KestrelSettings.ListenIp), 443, listenOptions =>
                        {
                            listenOptions.UseHttps(App.KestrelSslCertificate);
                        });

                    }
                })
                .UseStartup<Startup>();
        }
    }
}
