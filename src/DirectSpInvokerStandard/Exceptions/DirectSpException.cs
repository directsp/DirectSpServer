using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

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
            var settings = new JsonSerializerSettings();
            if (UseCamelCase)
                settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(SpCallError, settings);
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
                    ErrorId = _SpCallError.ErrorId,
                    ErrorMessage = _SpCallError.ErrorMessage,
                    ErrorDescription = _SpCallError.ErrorDescription,
                    ErrorProcName = _SpCallError.ErrorProcName,
                    ErrorData = _SpCallError.ErrorData
                };

                if (UseCamelCase && spCallError.ErrorData is JToken)
                       Util.CamelizeJToken(spCallError.ErrorData as JToken);

                return spCallError;
            }
            private set
            {
                _SpCallError = value;
            }
        }
    }
}
