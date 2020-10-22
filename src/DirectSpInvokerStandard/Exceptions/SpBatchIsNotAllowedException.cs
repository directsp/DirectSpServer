namespace DirectSp.Exceptions
{
    public class SpBatchIsNotAllowedException : DirectSpException
    {
        public SpBatchIsNotAllowedException(DirectSpException baseException) : base(baseException) { }

        public SpBatchIsNotAllowedException(string spName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.BatchIsNotAllowed.ToString(), ErrorNumber = (int)SpCommonExceptionId.BatchIsNotAllowed, ErrorMessage = $"Batch invoke is not allowed for {spName}" })
        {
        }
    }


}
