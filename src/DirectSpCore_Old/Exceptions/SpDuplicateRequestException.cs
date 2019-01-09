using System;

namespace DirectSp.Core.Exceptions
{
    public class SpDuplicateRequestException : Exception
    {
        public SpDuplicateRequestException(string requestId, Exception innerException = null) :
            base($"The request has been duplicated for \"{requestId}\" ", innerException)
        { }

    }
}
