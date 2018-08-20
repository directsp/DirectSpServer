using System;

namespace DirectSp.Core.Exceptions
{
    public class DuplicateRequestException : Exception
    {
        public DuplicateRequestException(string requestId, Exception innerException = null) :
            base($"The request has been duplicated for \"{requestId}\" ", innerException)
        { }

    }
}
