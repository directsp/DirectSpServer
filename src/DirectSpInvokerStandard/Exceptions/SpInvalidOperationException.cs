namespace DirectSp.Exceptions
{
    public class SpInvalidOperationException : DirectSpException
    {
        public SpInvalidOperationException(DirectSpException baseException) : base(baseException) { }

        public SpInvalidOperationException(string message)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.InvalidOperation.ToString(), ErrorId = (int)SpCommonExceptionId.InvalidOperation, ErrorMessage = message })
        {
        }
    }


}
