using DirectSp.Entities;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DirectSp.Host.Controllers
{
    public class ApiController : DirectSp.Controllers.ApiController
    {
        protected override Invoker Invoker { get { return App.Invoker; } }
        private ILog _logger;

        public ApiController()
        {
            _logger = Logger.Current;
        }

        [HttpPost, Authorize]
        [Route("[controller]/{method}")]
        public new async Task<IActionResult> Invoke(string method, [FromBody] InvokeParams invokeParams)
        {
            var userIdentifier = User.Identity.IsAuthenticated ? User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value : null;
            var reqId = Guid.NewGuid();
            var watch = new Stopwatch();
            watch.Start();
            _logger.Info($"{reqId}\t{method}\t\t{string.Join(',', invokeParams.SpCall.Params.Select(p => $"{p.Key}:{p.Value?.ToString().Replace("\n", "").Replace("\r", "")}"))}\t{userIdentifier}");

            var result = await base.Invoke(method, invokeParams);

            watch.Stop();
            _logger.Info($"{reqId}\t\t{watch.ElapsedMilliseconds}");
            return result;
        }

        [HttpPost, Authorize]
        [Route("[controller]")]
        public async Task<IActionResult> Invoke([FromBody] InvokeParams invokeParams)
        {
            var userIdentifier = User.Identity.IsAuthenticated ? User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value : null;
            var reqId = Guid.NewGuid();
            var watch = new Stopwatch();
            watch.Start();
            _logger.Info($"{reqId}\t{invokeParams.SpCall.Method}\t\t{string.Join(',', invokeParams.SpCall.Params.Select(p => $"{p.Key}:{p.Value?.ToString().Replace("\n", "").Replace("\r", "")}"))}\t{userIdentifier}");

            var result = await base.Invoke(invokeParams);

            watch.Stop();
            _logger.Info($"{reqId}\t\t{watch.ElapsedMilliseconds}");
            return result;
        }

        [HttpPost, Authorize]
        [Route("[controller]/invokebatch")]
        public new async Task<IActionResult> InvokeBatch([FromBody] InvokeParamsBatch invokeParamsBatch)
        {
            var userIdentifier = User.Identity.IsAuthenticated ? User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value : null;
            var reqId = Guid.NewGuid();
            var watch = new Stopwatch();
            watch.Start();
            _logger.Info($"{reqId}\t{nameof(InvokeBatch)}\t\t\t\t{userIdentifier}");

            var result = await base.InvokeBatch(invokeParamsBatch);
            
            watch.Stop();
            _logger.Info($"{reqId}\t\t{watch.ElapsedMilliseconds}");
            return result;
        }

        [HttpGet]
        [Route("[controller]/download/recordset")]
        public new async Task<IActionResult> DownloadRecordset(string id, string fileName)
        {
            var userIdentifier = User.Identity.IsAuthenticated ? User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value : null;
            var reqId = Guid.NewGuid();
            var watch = new Stopwatch();
            watch.Start();
            _logger.Info($"{reqId}\t{nameof(DownloadRecordset)}\t\t{nameof(id)}:{id},{nameof(fileName)}:{fileName}\t{userIdentifier}");

            var result = await base.DownloadRecordset(id, fileName);

            watch.Stop();
            _logger.Info($"{reqId}\t\t{watch.ElapsedMilliseconds}");
            return result;
        }
    }
}
