using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Net;

namespace DirectSp.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static void ConfigKestrel(WebHostBuilderContext builderContext, KestrelServerOptions options)
        {
            // find local endpoint to listen
            var endPoint = App.KestrelSettings.EndPoint;
            var appUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrEmpty(appUrl))
            {
                var uri = new Uri(appUrl);
                endPoint = IPEndPoint.Parse($"{uri.Host}:{uri.Port}");
            }

            // listen to endpoint
            if (endPoint != null)
            {
                options.Listen(endPoint, listenOptions =>
                {
                    listenOptions.UseHttps(App.KestrelCertificate);
                });
            }
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return
                WebHost.CreateDefaultBuilder(args)
                .UseKestrel(ConfigKestrel)
                .UseStartup<Startup>();
        }
    }
}
