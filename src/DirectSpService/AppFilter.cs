using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSpService
{
    public class AppFilter
    {
        private readonly RequestDelegate _next;

        public AppFilter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // put delay for invoke call
            if (App.AppSettings.InvokeDelayInterval != 0)
            {
                var interval = App.AppSettings.InvokeDelayInterval * 1000;
                var delay = new Random().Next(interval / 2, interval + interval / 2);
                Thread.Sleep(delay);
            }

            await _next(context);
        }
    }

    public static class AppFilterExtension
    {
        public static IApplicationBuilder UseAppFilter(this IApplicationBuilder app, IHostingEnvironment env)
        {
            return app.UseMiddleware<AppFilter>();
        }
    }

}
