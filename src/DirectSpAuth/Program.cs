using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Net;

namespace DirectSp.AuthServer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    if (!string.IsNullOrEmpty(App.KestlerSettings.ListenIp))
                    {
                        options.Listen(IPAddress.Parse(App.KestlerSettings.ListenIp), 443, listenOptions =>
                        {
                            listenOptions.UseHttps(App.KestrelSslCertificate);
                        });
                    }
                })
                .UseSetting("detailedErrors", "true")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }


    }
}
