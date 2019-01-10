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
        private Invoker Invoker { get; }

        public DirectSpHttpHandler(Invoker invoker)
        {
            Invoker = invoker;
        }

        public async Task<HttpResponseMessage> Process(HttpRequestMessage requestMessage)
        {
            // parse request
            var json = await requestMessage.Content.ReadAsStringAsync();
            var invokeParams = JsonConvert.DeserializeObject<InvokeParams>(json);
            var spInvokeParams = new SpInvokeParams
            {
                AuthUserId = (string)requestMessage.Properties["AuthUserId"],
                UserRemoteIp = ((IPEndPoint)requestMessage.Properties["RemoteEndPoint"]).Address.ToString(),
                InvokeOptions = invokeParams.InvokeOptions,
            };

            // prepare json serialize
            var jsonSerializerSettings = new JsonSerializerSettings();
            if (Invoker.UseCamelCase)
                jsonSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            // process
            var response = new HttpResponseMessage();
            try
            {
                //invoke
                var result = await Invoker.Invoke(invokeParams.SpCall, spInvokeParams);
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
            response.Headers.Add("DSP-AppVersion", Invoker.AppVersion);
            return response;
        }
    }
}
