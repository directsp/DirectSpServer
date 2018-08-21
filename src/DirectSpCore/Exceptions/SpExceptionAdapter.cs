using DirectSp.Core.Entities;
using System;

namespace DirectSp.Core.Exceptions
{
    internal enum SpCommonExceptionId
    {
        GeneralException = 55001,
        AccessDeniedOrObjectNotExists = 55002,
        ObjectAlreadyExists = 55004,
        ObjectIsInUse = 55005,
        InvalidArgument = 55011,
        FatalError = 55012,
        InvalidOperation = 55016,
        NotSupported = 55017,
        NotImplemeted = 55018,
        UserIsDisabled = 55019,
        AmbiguousException = 55020,
        NoOperation = 55021,
        InvalidCaptcha = 55022,
        BatchIsNotAllowed = 55023,
        TooManyRequest = 55024,
        AuthUserNotFound = 55025,
        InvokerAppVersion = 55026,
        Maintenance = 55027,
        MaintenanceReadOnly = 55028,
        InvalidParamSignature = 55029,
    }

    internal static class SpExceptionAdapter
    {
        public static SpException Convert(SpInvoker spInvoker, Exception ex)
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
                    return new SpInvalidCaptchaException(spInvoker.CaptchaHandler.Create().Result, ret);

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
