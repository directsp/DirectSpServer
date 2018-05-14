using DirectSp.Core.Entities;
using DirectSp.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DirectSp.Core.Exceptions
{
    public class SpAccessDeniedOrObjectNotExistsException : SpException
    {
        public SpAccessDeniedOrObjectNotExistsException(SpException baseException) : base(baseException) { }
    }

    public class SpInvokerAppVersionException : SpException
    {
        public SpInvokerAppVersionException(SpException baseException) : base(baseException) { }
    }

    public class SpInvalidOperationException : SpException
    {
        public SpInvalidOperationException(SpException baseException) : base(baseException) { }

        public SpInvalidOperationException(string message)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.InvalidOperation.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidOperation, ErrorMessage = message })
        {
        }
    }

    public class SpBatchIsNotAllowedException : SpException
    {
        public SpBatchIsNotAllowedException(SpException baseException) : base(baseException) { }

        public SpBatchIsNotAllowedException(string spName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.BatchIsNotAllowed.ToString(), ErrorNumber = (int)SpCommonExceptionId.BatchIsNotAllowed, ErrorMessage = $"Batch invoke is not allowed for {spName}" })
        {
        }
    }

}
