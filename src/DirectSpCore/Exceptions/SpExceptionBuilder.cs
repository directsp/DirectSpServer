using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System;
using System.Data.SqlClient;

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
    }

    internal static class SpExceptionBuilder
    {
        public static SpException Create(SpInvoker spInvokerInternal, Exception ex)
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
                    return new SpInvalidCaptchaException(spInvokerInternal, ret);

                default:
                    return ret;
            }
        }

    }
}
