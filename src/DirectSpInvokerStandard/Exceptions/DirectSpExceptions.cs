namespace DirectSp.Exceptions
{
    public class SpAccessDeniedOrObjectNotExistsException : DirectSpException
    {
        public SpAccessDeniedOrObjectNotExistsException(DirectSpException baseException) : base(baseException) { }
        public SpAccessDeniedOrObjectNotExistsException()
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.AccessDeniedOrObjectNotExists.ToString(), ErrorNumber = (int)SpCommonExceptionId.AccessDeniedOrObjectNotExists }) { }
    }

    public class SpObjectAlreadyExists : DirectSpException
    {
        public SpObjectAlreadyExists(DirectSpException baseException) : base(baseException) { }
        public SpObjectAlreadyExists()
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.ObjectAlreadyExists.ToString(), ErrorNumber = (int)SpCommonExceptionId.ObjectAlreadyExists }) { }
    }

    public class SpInvokerAppVersionException : DirectSpException
    {
        public SpInvokerAppVersionException(DirectSpException baseException) : base(baseException) { }
    }

    public class SpMaintenanceException : DirectSpException
    {
        public SpMaintenanceException(DirectSpException baseException) : base(baseException) { }
    }

    public class SpMaintenanceReadOnlyException : DirectSpException
    {
        public SpMaintenanceReadOnlyException(DirectSpException baseException) : base(baseException) { }
        public SpMaintenanceReadOnlyException(string spName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.MaintenanceReadOnly.ToString(), ErrorNumber = (int)SpCommonExceptionId.MaintenanceReadOnly, ErrorMessage = $"{spName} cannot be called in readonly mode!" })
        {
        }
    }

    public class SpInvalidOperationException : DirectSpException
    {
        public SpInvalidOperationException(DirectSpException baseException) : base(baseException) { }

        public SpInvalidOperationException(string message)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.InvalidOperation.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidOperation, ErrorMessage = message })
        {
        }
    }

    public class SpBatchIsNotAllowedException : DirectSpException
    {
        public SpBatchIsNotAllowedException(DirectSpException baseException) : base(baseException) { }

        public SpBatchIsNotAllowedException(string spName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.BatchIsNotAllowed.ToString(), ErrorNumber = (int)SpCommonExceptionId.BatchIsNotAllowed, ErrorMessage = $"Batch invoke is not allowed for {spName}" })
        {
        }
    }

    public class SpInvalidParamSignature : DirectSpException
    {
        public SpInvalidParamSignature(DirectSpException baseException) : base(baseException) { }

        public SpInvalidParamSignature(string paramName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.BatchIsNotAllowed.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidParamSignature, ErrorMessage = $"Invalid parameter singnature for {paramName}" })
        {
        }
    }


}
