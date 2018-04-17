using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace DirectSp.Core.Exceptions
{
    public class SpException : Exception
    {
        public static bool UseCamelCase { get; set; } = true;

         protected SpException(SpException baseException)
            : this(baseException.SpCallError, baseException.InnerException)
        {
            StatusCode = baseException.StatusCode;
        }

        public SpException(Exception ex)
            : base(null, ex)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
            var spCallError = new SpCallError() { ErrorType = ex.GetType().Name, ErrorName = ex.GetType().ToString(), ErrorMessage = ex.Message, ErrorDescription = ex.ToString() };

            if (ex is SqlException sqlException)
            {
                //set status
                StatusCode = StatusCodes.Status400BadRequest;
                if (sqlException.Number == (int)SpCommonExceptionId.AccessDeniedOrObjectNotExists) StatusCode = StatusCodes.Status401Unauthorized;

                //try to parse error info
                try
                {
                    spCallError = JsonConvert.DeserializeObject<SpCallError>(ex.Message);
                    spCallError.ErrorType = typeof(SpException).Name;
                }
                catch { }
                spCallError.ErrorNumber = sqlException.Number;
            }

            SpCallError = spCallError;
        }

        public SpException(string message, int status = StatusCodes.Status400BadRequest)
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
            StatusCode = StatusCodes.Status400BadRequest;
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
