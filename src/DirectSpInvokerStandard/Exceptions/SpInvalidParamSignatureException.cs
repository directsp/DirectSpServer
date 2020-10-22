namespace DirectSp.Exceptions
{
    public class SpInvalidParamSignatureException : DirectSpException
    {
        public SpInvalidParamSignatureException(DirectSpException baseException) : base(baseException) { }

        public SpInvalidParamSignatureException(string paramName)
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.InvalidParamSignature.ToString(), ErrorNumber = (int)SpCommonExceptionId.InvalidParamSignature, ErrorMessage = $"Invalid parameter singnature for {paramName}" })
        {
        }
    }


}
