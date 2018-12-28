namespace DirectSp.Core.Exceptions
{
    public enum SpCommonExceptionId
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
}
