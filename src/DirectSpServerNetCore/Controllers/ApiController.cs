using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System;
using Microsoft.AspNetCore.Mvc;
using DirectSp.Entities;
using Microsoft.AspNetCore.Http.Extensions;
using DirectSp.Exceptions;

namespace DirectSp.Controllers
{
    public abstract class ApiController : Controller
    {
        protected abstract DirectSpInvoker Invoker { get; }

        public async Task<IActionResult> Invoke(string method, [FromBody] InvokeParams invokeParams)
        {
            return await Invoke(invokeParams);
        }

        public async Task<IActionResult> Invoke([FromBody] InvokeParams invokeParams, bool isSystem = false)
        {
            try
            {
                //invoke
                var spInvokeParams = new SpInvokeParams
                {
                    AuthUserId = isSystem || !User.Identity.IsAuthenticated ? null : Util.GetClaimUserId(User),
                    UserRemoteIp = HttpContext.Connection.RemoteIpAddress.ToString(),
                    InvokeOptions = invokeParams.InvokeOptions,
                    RecordsetDownloadUrlTemplate = UriHelper.BuildAbsolute(scheme: Request.Scheme, host: Request.Host, path: "/api/download/recordset") + "?id={id}&filename={filename}",
                };
                var res = await Invoker.Invoke(invokeParams.SpCall, spInvokeParams, isSystem);

                AddResponseHeaders();
                return JsonHelper(res);
            }
            catch (DirectSpException ex)
            {
                AddResponseHeaders();
                return StatusCode((int)ex.StatusCode, ex.SpCallError);
            }
            catch (Exception err)
            {
                var ex = new DirectSpException(err);
                return StatusCode((int)ex.StatusCode, ex.SpCallError);
            }
        }

        public async Task<IActionResult> InvokeBatch([FromBody] InvokeParamsBatch invokeParamsBatch)
        {
            try
            {
                var spInvokeParams = new SpInvokeParams
                {
                    AuthUserId = Util.GetClaimUserId(User),
                    UserRemoteIp = HttpContext.Connection.RemoteIpAddress.ToString(),
                    InvokeOptions = invokeParamsBatch.InvokeOptions,
                    RecordsetDownloadUrlTemplate = UriHelper.BuildAbsolute(scheme: Request.Scheme, host: Request.Host, path: "/api/download/recordset?id={id}&filename={filename}"),
                };

                var res = await Invoker.Invoke(invokeParamsBatch.SpCalls, spInvokeParams);

                AddResponseHeaders();
                return JsonHelper(res);
            }
            catch (DirectSpException ex)
            {
                AddResponseHeaders();
                return StatusCode((int)ex.StatusCode, ex.SpCallError);
            }
            catch (Exception err)
            {
                var ex = new DirectSpException(err);
                return StatusCode((int)ex.StatusCode, ex.SpCallError);
            }
        }

        public Task<IActionResult> DownloadRecordset(string id, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = "result.csv";

                AddResponseHeaders();

                //get file
                var filePath = Path.Combine(Invoker.InvokerPath.RecordsetsFolder, id);
                var fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                return Task.FromResult<IActionResult>(File(fs, "text/csv", fileName));
            }
            catch (SpAccessDeniedOrObjectNotExistsException)
            {
                AddResponseHeaders();
                return Task.FromResult<IActionResult>(new NotFoundResult());
            }
            catch (DirectSpException ex)
            {
                AddResponseHeaders();
                return Task.FromResult<IActionResult>(StatusCode((int)ex.StatusCode, ex.SpCallError));
            }
            catch (Exception err)
            {
                var ex = new DirectSpException(err);
                return Task.FromResult<IActionResult>(StatusCode((int)ex.StatusCode, ex.SpCallError));
            }
        }

        private JsonResult JsonHelper(object data)
        {
            var serializerSettings = new JsonSerializerSettings();
            if (Invoker.UseCamelCase)
                serializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            return base.Json(data, serializerSettings);
        }

        private void AddResponseHeaders()
        {
            //set app version
            Request.HttpContext.Response.Headers.Add("DSP-AppVersion", Invoker.AppVersion);
        }

    }
}
