using DirectSp.Core.Entities;
using System;

namespace DirectSp.Core.Exceptions
{
    internal static class SpExceptionAdapter
    {
        public static SpException Convert(Exception ex, CaptchaController captchaController)
        {
            if (ex is SpException)
                return (SpException)ex;

            //create invoker exception
            var ret = new SpException(ex);

            // Convert to Sp* exceptions
            switch (ret.SpCallError.ErrorNumber)
            {
                case (int)SpCommonExceptionId.AccessDeniedOrObjectNotExists:
                    return new SpAccessDeniedOrObjectNotExistsException(ret);

                case (int)SpCommonExceptionId.InvalidCaptcha:
                    return new SpInvalidCaptchaException(captchaController.Create().Result, ret);

                case (int)SpCommonExceptionId.InvokerAppVersion:
                    return new SpInvokerAppVersionException(ret);

                case (int)SpCommonExceptionId.Maintenance:
                    return new SpMaintenanceException(ret);

                case (int)SpCommonExceptionId.MaintenanceReadOnly:
                    return new SpMaintenanceReadOnlyException(ret);

                case (int)SpCommonExceptionId.InvalidParamSignature:
                    return new SpInvalidParamSignature(ret);

                case (int)SpCommonExceptionId.ObjectAlreadyExists:
                    return new SpObjectAlreadyExists(ret);

                default:
                    return ret;
            }
        }
    }
}
