using DirectSp;
using DirectSp.Entities;
using DirectSp.Exceptions;
using DirectSp.Providers;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DirectSp
{
    public class DirectSpHttpHandler
    {
        private readonly string _basePath;
        private readonly DirectSpInvoker _invoker;

        public DirectSpHttpHandler(DirectSpInvoker invoker, string basePath)
        {
            _invoker = invoker;
            _basePath = basePath;
        }

        private bool IsUriMatch(Uri uri)
        {
            var path = uri.AbsolutePath;
            return path.IndexOf("/" + _basePath + "/") == 0 || path == "/" + _basePath;
        }

        public async Task<HttpResponseMessage> Process(HttpRequestMessage requestMessage)
        {
            if (!IsUriMatch(requestMessage.RequestUri))
                return null;

            // parse request
            var json = await requestMessage.Content.ReadAsStringAsync();
            var invokeParams = JsonConvert.DeserializeObject<InvokeParams>(json);
            var spInvokeParams = new SpInvokeParams
            {
                AuthUserId = (string)requestMessage.Properties["AuthUserId"] ?? "$$",
                UserRemoteIp = ((IPEndPoint)requestMessage.Properties["RemoteEndPoint"]).Address.ToString(),
                InvokeOptions = invokeParams.InvokeOptions,
            };

            // prepare json serialize
            var jsonSerializerSettings = new JsonSerializerSettings();
            if (_invoker.UseCamelCase)
                jsonSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            // process
            var response = new HttpResponseMessage();
            try
            {
                //invoke
                var result = await _invoker.Invoke(invokeParams.SpCall, spInvokeParams);
                response.Content = new StringContent(JsonConvert.SerializeObject(result, jsonSerializerSettings), System.Text.Encoding.UTF8, "application/json");
                response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                var dspError = ex is DirectSpException ? (DirectSpException)ex: new DirectSpException(ex);
                response.Content = new StringContent(JsonConvert.SerializeObject(dspError.SpCallError, jsonSerializerSettings));
                response.StatusCode = dspError.StatusCode;
            }

            //add headers
            response.Headers.Add("DSP-AppVersion", _invoker.AppVersion);
            return response;
        }
    }
}
