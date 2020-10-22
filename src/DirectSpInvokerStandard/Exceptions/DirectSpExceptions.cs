namespace DirectSp.Exceptions
{

    public class DirectSpExceptions : DirectSpException
    {
        public DirectSpExceptions(DirectSpException baseException) : base(baseException) { }

        public DirectSpExceptions(string paramName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.BatchIsNotAllowed.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidParamSignature, ErrorMessage = $"Invalid parameter singnature for {paramName}" })
        {
        }
    }


}
