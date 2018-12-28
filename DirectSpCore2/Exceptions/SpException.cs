using System;
using Newtonsoft.Json;
using DirectSp.Core.Entities;
using Newtonsoft.Json.Linq;

namespace DirectSp.Core.Exceptions
{
    public class SpException : Exception
    {
        public const int Status400BadRequest = 400;
        public const int Status429TooManyRequests = 429;


        public static bool UseCamelCase { get; set; } = true;

         protected SpException(SpException baseException)
            : this(baseException.SpCallError, baseException.InnerException)
        {
            StatusCode = baseException.StatusCode;
        }

        public SpException(Exception ex)
            : base(null, ex)
        {
            StatusCode = Status400BadRequest;
            SpCallError = new SpCallError() { ErrorType = ex.GetType().Name, ErrorName = ex.GetType().ToString(), ErrorMessage = ex.Message, ErrorDescription = ex.ToString() }; ;
        }

        public SpException(string message, int status = Status400BadRequest)
        {
            SpCallError = new SpCallError()
            {
                ErrorType = GetType().Name,
                ErrorMessage = message
            };
            StatusCode = status;
        }

        public SpException(SpCallError spCallError, Exception innerException = null)
            : base(null, innerException)
        {
            StatusCode = Status400BadRequest;
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

        public int StatusCode { get; protected set; }

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
