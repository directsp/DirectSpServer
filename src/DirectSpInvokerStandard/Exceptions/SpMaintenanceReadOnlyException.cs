namespace DirectSp.Exceptions
{
    public class SpMaintenanceReadOnlyException : DirectSpException
    {
        public SpMaintenanceReadOnlyException(DirectSpException baseException) : base(baseException) { }
        public SpMaintenanceReadOnlyException(string spName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.MaintenanceReadOnly.ToString(), ErrorId = (int)SpCommonExceptionId.MaintenanceReadOnly, ErrorMessage = $"{spName} cannot be called in readonly mode!" })
        {
        }
    }


}
