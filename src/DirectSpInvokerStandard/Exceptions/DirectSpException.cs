using System;
using System.Net;
using System.Text.Json;

namespace DirectSp.Exceptions
{
    public class DirectSpException : Exception
    {
        public static bool UseCamelCase { get; set; } = true;

         protected DirectSpException(DirectSpException baseException)
            : this(baseException.SpCallError, baseException.InnerException)
        {
            StatusCode = baseException.StatusCode;
        }

        public DirectSpException(Exception ex)
            : base(null, ex)
        {
            StatusCode = HttpStatusCode.BadRequest;
            SpCallError = new SpCallError() { ErrorType = ex.GetType().Name, ErrorName = ex.GetType().ToString(), ErrorMessage = ex.Message, ErrorDescription = ex.ToString() }; ;
        }

        public DirectSpException(string message, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            SpCallError = new SpCallError()
            {
                ErrorType = GetType().Name,
                ErrorMessage = message
            };
            StatusCode = status;
        }

        public DirectSpException(SpCallError spCallError, Exception innerException = null)
            : base(null, innerException)
        {
            StatusCode = HttpStatusCode.BadRequest;
            SpCallError = spCallError;
        }

        public override string Message
        {
            get
            {
                return SpCallError.ErrorMessage;
            }
        }

        public override string ToString()
        {
            var settings = new JsonSerializerOptions();
            if (UseCamelCase)
                settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return JsonSerializer.Serialize(SpCallError, settings);
        }

        public HttpStatusCode StatusCode { get; protected set; }

        protected SpCallError _SpCallError;
        public SpCallError SpCallError
        {
            get
            {
                var spCallError = new SpCallError()
                {
                    ErrorType = _SpCallError.ErrorType,
                    ErrorName = _SpCallError.ErrorName,
                    ErrorNumber = _SpCallError.ErrorNumber,
                    ErrorMessage = _SpCallError.ErrorMessage,
                    ErrorDescription = _SpCallError.ErrorDescription,
                    ErrorProcName = _SpCallError.ErrorProcName,
                    ErrorData = _SpCallError.ErrorData
                };

                if (UseCamelCase && spCallError.ErrorData is JsonElement)
                       Util.CamelizeJElement((JsonElement)spCallError.ErrorData);

                return spCallError;
            }
            private set
            {
                _SpCallError = value;
            }
        }
    }
}
