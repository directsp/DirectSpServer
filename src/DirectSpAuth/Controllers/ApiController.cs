using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DirectSp.Core.Entities;
using System.Threading.Tasks;
using DirectSp.Core;

namespace DirectSp.AuthServer.Controllers
{
    public class ApiController : DirectSp.Core.Controllers.ApiController
    {
        protected override SpInvoker SpInvoker { get { return App.SpInvoker; } }


        [HttpPost]
        [Route("[controller]/{method}")]
        public new async Task<IActionResult> Invoke(string method, [FromBody] InvokeParams invokeParams)
        {
            return await Invoke(invokeParams); //call same class not parent
        }

        [HttpPost]
        [Route("[controller]")]
        public async Task<IActionResult> Invoke([FromBody] InvokeParams invokeParams)
        {
            bool isSystem = invokeParams.SpCall.Method == "User_AccountRecoveryMethods" || invokeParams.SpCall.Method == "User_RecoverAccount";
            
            //Check Authentication
            if (!isSystem && !User.Identity.IsAuthenticated)
                return Unauthorized();

            return await base.Invoke(invokeParams, isSystem);
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
