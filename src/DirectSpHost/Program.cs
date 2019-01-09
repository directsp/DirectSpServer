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
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
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
                .UseStartup<Startup>();
        }
    }
}
