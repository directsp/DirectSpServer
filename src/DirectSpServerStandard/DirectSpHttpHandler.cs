using DirectSp.Entities;
using DirectSp.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

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

        private string _DownloadRecordsetPath => $"/{_basePath}/download/recordset";

        public async Task<HttpResponseMessage> Process(HttpRequestMessage requestMessage)
        {
            if (!IsUriMatch(requestMessage.RequestUri))
                return null;

            if (requestMessage.RequestUri.AbsolutePath.Equals(_DownloadRecordsetPath, StringComparison.InvariantCultureIgnoreCase))
                return DownloadRecorset(requestMessage);

            // parse request
            var json = await requestMessage.Content.ReadAsStringAsync();
            var invokeParams = JsonConvert.DeserializeObject<InvokeParams>(json);
            var spInvokeParams = new SpInvokeParams
            {
                AuthUserId = (string)requestMessage.Properties["AuthUserId"] ?? "$$",
                UserRemoteIp = ((IPEndPoint)requestMessage.Properties["RemoteEndPoint"]).Address.ToString(),
                InvokeOptions = invokeParams.InvokeOptions,
                RecordsetDownloadUrlTemplate = new UriBuilder(requestMessage.RequestUri) { Path = _DownloadRecordsetPath, Query = "id={id}&filename={filename}" }.ToString(),
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
                var dspError = ex is DirectSpException ? (DirectSpException)ex : new DirectSpException(ex);
                response.Content = new StringContent(JsonConvert.SerializeObject(dspError.SpCallError, jsonSerializerSettings));
                response.StatusCode = dspError.StatusCode;
            }

            //add headers
            response.Headers.Add("DSP-AppVersion", _invoker.AppVersion);
            return response;
        }

        private HttpResponseMessage DownloadRecorset(HttpRequestMessage requestMessage)
        {
            try
            {
                var queryParams = HttpUtility.ParseQueryString(requestMessage.RequestUri.Query);
                var id = queryParams.Get("id");
                var fileName = queryParams.Get("filename");
                if (id == null || fileName == null)
                    throw new FileNotFoundException();

                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = "result.csv";

                // Check file existance
                var filePath = Path.Combine(_invoker.InvokerPath.RecordsetsFolder, id);
                if (!File.Exists(filePath))
                    throw new FileNotFoundException();

                // Create content
                var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var response = new HttpResponseMessage
                {
                    Content = new StreamContent(fs),
                    StatusCode = HttpStatusCode.OK
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

                //Add headers
                response.Headers.Add("DSP-AppVersion", _invoker.AppVersion);
                response.Content.Headers.Add("content-disposition", $"attachment; filename=\"{fileName}\"");
                return response;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
