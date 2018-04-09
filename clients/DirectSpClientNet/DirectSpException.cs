using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Client
{
    class DirectSpException : Exception
    {
        public override string Message
        {
            get
            {
                var ret = errorName ?? "";
                if (!string.IsNullOrWhiteSpace(ret)) ret += "; ";
                if (!string.IsNullOrWhiteSpace(errorMessage)) return ret + errorMessage;
                if (!string.IsNullOrWhiteSpace(errorDescription)) return ret + errorDescription;
                return ret;
            }
        }

        internal static DirectSpException fromHttpResponse(HttpResponseMessage httpResponse)
        {
            if (httpResponse.IsSuccessStatusCode)
                throw new InvalidOperationException("fromHttpResponse should not be called on success mode");

            var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
            return fromHttpResponse(httpResponse.StatusCode, responseContent);
        }

        internal static DirectSpException fromHttpResponse(HttpStatusCode statusCode, string content)
        {
            //parse content as OpenId exception
            try
            {
                var obj = JObject.Parse(content);
                var ex = new DirectSpException()
                {
                    statusCode = statusCode,
                    errorName = obj["error"].Value<string>(),
                    errorDescription = obj["error_description"].Value<string>(),
                };

                // recover from ErrorDescription
                try
                {
                    ex = fromSpException(statusCode, ex.errorDescription);
                }
                catch { }
                return ex;
            }
            catch { }

            //parse content as SpException exception
            try
            {
                return fromSpException(statusCode, content);
            }
            catch { }

            // simple text message
            var ret = new DirectSpException()
            {
                statusCode = statusCode,
                errorMessage = content
            };

            return ret;
        }

        private static DirectSpException fromSpException(HttpStatusCode statusCode, string content)
        {
            var spCallError = JsonConvert.DeserializeObject<SpCallError>(content);
            var ret = new DirectSpException()
            {
                statusCode = statusCode,
                errorType = spCallError.errorType,
                errorName = spCallError.errorName,
                errorNumber = spCallError.errorNumber,
                errorMessage = spCallError.errorMessage,
                errorDescription = spCallError.errorDescription,
                errorProcName = spCallError.errorProcName,
                errorData = spCallError.errorData,
            };
            return ret;
        }

        public string errorType { get; internal set; }
        public string errorName { get; internal set; }
        public int errorNumber { get; internal set; }
        public string errorMessage { get; internal set; }
        public string errorDescription { get; internal set; }
        public string errorProcName { get; internal set; }
        public object errorData { get; internal set; }
        public HttpStatusCode statusCode { get; internal set; }
    }
}
