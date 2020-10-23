namespace DirectSp.Exceptions
{
    public class SpObjectAlreadyExists : DirectSpException
    {
        public SpObjectAlreadyExists(DirectSpException baseException) : base(baseException) { }
        public SpObjectAlreadyExists()
            : base(new SpCallError() { ErrorName = SpCommonExceptionId.ObjectAlreadyExists.ToString(), ErrorId = (int)SpCommonExceptionId.ObjectAlreadyExists }) { }
    }


}
