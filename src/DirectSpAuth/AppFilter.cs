using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DirectSpAuth
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
            //Put SpApp_Authorization to header as Authorization if Authorization is not set
            if (context.Request.Method.Equals("POST", StringComparison.Ordinal) && string.IsNullOrEmpty(context.Request.Headers["authorization"]))
            {
                var SpApp_Authorization = context.Request.Form["SpApp_Authorization"];
                if (!string.IsNullOrEmpty(SpApp_Authorization))
                    context.Request.Headers.Add("Authorization", SpApp_Authorization);
            }

            //put delay for Invoke
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
