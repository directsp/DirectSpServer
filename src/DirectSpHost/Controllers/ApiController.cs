using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DirectSp.Core.Entities;
using System.Threading.Tasks;
using DirectSp.Core;
using System.Text.Encodings.Web;

namespace DirectSp.Host.Controllers
{
    public class ApiController : DirectSp.Core.Controllers.ApiController
    {
        protected override SpInvoker SpInvoker { get { return App.SpInvoker; } }


        [HttpPost, Authorize]
        [Route("[controller]/{method}")]
        public new async Task<IActionResult> Invoke(string method, [FromBody] InvokeParams invokeParams)
        {
            return await base.Invoke(method, invokeParams);
        }

        [HttpPost, Authorize]
        [Route("[controller]")]
        public async Task<IActionResult> Invoke([FromBody] InvokeParams invokeParams)
        {
            return await base.Invoke(invokeParams);
        }

        [HttpPost, Authorize]
        [Route("[controller]/invokebatch")]
        public new async Task<IActionResult> InvokeBatch([FromBody] InvokeParamsBatch invokeParamsBatch)
        {
            return await base.InvokeBatch(invokeParamsBatch);
        }

        [HttpGet]
        [Route("[controller]/download/recordset")]
        public new async Task<IActionResult> DownloadRecordset(string id, string fileName)
        {
            return await base.DownloadRecordset(id, fileName);
        }
    }
}
