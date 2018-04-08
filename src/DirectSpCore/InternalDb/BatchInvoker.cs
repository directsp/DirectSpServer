using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Core.InternalDb
{
    public class BatchInvoker
    {
        //public SqInvoker SpInvoker { get; private set; }
        //public ClaimsPrincipal User { get; private set; }
        //public string BatchInvokerId { get; private set; }
        //public InvokeParamsBatch InvokeParamsBatch { get; private set; }

        //public static async Task<BatchInvoker> Create(SqInvoker spInvoker, ClaimsPrincipal user, InvokeParamsBatch invokeParamsBatch, string batchName)
        //{
        //    var batchInvoker = new BatchInvoker()
        //    {
        //        SpInvoker = spInvoker,
        //        User = user,
        //        InvokeParamsBatch = invokeParamsBatch,
        //    };

        //    var spCall = new SpCall() { Method = "Batch_Create" };
        //    await spInvoker.Invoke(spCall);
        //    return new BatchInvoker();
        //}

        //public async Task Run()
        //{
        //    var spCalls = InvokeParamsBatch.SpCalls;
        //    for (var i = 0; i < spCalls.Length; i++)
        //    {
        //        var spCall = spCalls[i];
        //        try
        //        {
        //            //SpInvokeParams spInvokeParams = new SpInvokeParams()
        //            //{
        //            //    UserId = Util.GetClaimUserId(User),
        //            //}
        //            //var spCallResult = await SpInvoker.Invoke(User, spInvokeParams);
        //            await ItemProcessResultSet(i, spCallResult, null);
        //        }
        //        catch (SpException ex)
        //        {
        //            if (ex.StatusCode == StatusCodes.Status500InternalServerError)
        //                throw ex;

        //            //add to direct result
        //            await ItemProcessResultSet(i, null, ex);
        //        }
        //    }
        //}

        //private async Task ItemProcessResultSet(int itemIdex, SpCallResult spCallResult, SpException ex)
        //{
        //    var spCall = new SpCall() { Method = "Batch_ItemProcessResultSet" };
        //    await SpInvoker.Invoke(spCall);
        //    throw new NotImplementedException();
        //}

        //[HttpPost, Authorize]
        //[Route("[controller]/InvokeBatchJob")]
        //public async Task<IActionResult> InvokeBatchJob([FromBody] InvokeParamsBatch invokeApiParamsBatch)
        //{
        //    try
        //    {
        //        //validate options
        //        if (invokeParamsBatch.InvokeOptions.IsWithRecodsetDownloadUri)
        //            throw new InvalidOperationException("Can not use IsWithRecodsetDownloadUri for batch!");

        //        var batchInvoker = await BatchInvoker.Create(App.Invoker, User, invokeApiParamsBatch, null);
        //        Task.Factory.StartNew(() =>
        //        {
        //            batchInvoker.Run().Wait();
        //        }, TaskCreationOptions.LongRunning).GetAwaiter();

        //        var spCallResult = new SpCallResult
        //        {
        //            ["batchInvokerId"] = batchInvoker.BatchInvokerId
        //        };
        //        return Json(batchInvoker.BatchInvokerId);
        //    }
        //    catch (SpException ex)
        //    {
        //        return StatusCode(ex.StatusCode, ex.SpCallError);
        //    }
        //}


    }
}
