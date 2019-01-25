using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace DirectSp.Host
{
    public class DirectSpFilter
    {
        private readonly RequestDelegate _next;
        private readonly DirectSpHttpHandler _directSpHttpHandler;

        public DirectSpFilter(RequestDelegate next, DirectSpHttpHandler directSpHttpHandler)
        {
            _next = next;
            _directSpHttpHandler = directSpHttpHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestMessage = context.ToHttpRequestMessage();
            
            // set request properties
            requestMessage.Properties["RemoteEndPoint"] = new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort);
            requestMessage.Properties["AuthUserId"] = context.User.Identity.Name;

            var responseMessage = await _directSpHttpHandler.Process(requestMessage);
            if (responseMessage != null)
            {
                await context.FromHttpResponseMessage(responseMessage);
                return;
            }

            await _next(context);
        }
    }

    public static class DirectSpFilterExtension
    {
        public static IApplicationBuilder UseDirectSpFilter(this IApplicationBuilder app, DirectSpHttpHandler directSpHttpHandler)
        {
            return app.UseMiddleware<DirectSpFilter>(directSpHttpHandler);
        }
    }

}
