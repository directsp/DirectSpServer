using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DirectSp
{
    internal static class ProxyAdvancedExtensions
    {
        private const int StreamCopyBufferSize = 81920;

        public static HttpRequestMessage ToHttpRequestMessage(this HttpContext context)
        {
            var request = context.Request;
            var uri = new Uri(request.GetDisplayUrl());

            var requestMessage = new HttpRequestMessage();
            var requestMethod = request.Method;
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers
            foreach (var header in request.Headers)
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Headers.Host = uri.Authority;
            requestMessage.RequestUri = uri;
            requestMessage.Method = new HttpMethod(request.Method);
            requestMessage.Properties["MS_IsLocal"] = IPAddress.IsLoopback(request.HttpContext.Connection.RemoteIpAddress);
            requestMessage.Properties["RemoteEndPoint"] = new IPEndPoint(request.HttpContext.Connection.RemoteIpAddress, request.HttpContext.Connection.RemotePort);


            return requestMessage;
        }

        public static async Task FromHttpResponseMessage(this HttpContext context, HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
                throw new ArgumentNullException(nameof(responseMessage));

            var response = context.Response;

            response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }

            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
            response.Headers.Remove("transfer-encoding");

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                await responseStream.CopyToAsync(response.Body, StreamCopyBufferSize, context.RequestAborted);
            }
        }
    }
}
