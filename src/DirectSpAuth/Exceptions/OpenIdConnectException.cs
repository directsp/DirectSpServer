using AspNet.Security.OpenIdConnect.Primitives;
using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.AuthServer.Exceptions
{
    public class OpenIdConnectException : Exception
    {
        private SpCallError _SpCallError = new SpCallError();

        public OpenIdConnectException(string error, string description, Exception innerException = null) : base(description)
        {
            _SpCallError.ErrorType = GetType().Name;
            _SpCallError.ErrorNumber = 0;
            _SpCallError.ErrorName = error;
            _SpCallError.ErrorMessage = description;

            ExtractInnerError(description);
        }

        private void ExtractInnerError(string description)
        {
            if (description == null)
                return;

            //try to extract actual data from description
            try
            {
                _SpCallError = JsonConvert.DeserializeObject<SpCallError>(description);
                ExtractInnerError(_SpCallError.ErrorDescription);
            }
            catch { }
        }

        public override string ToString()
        {
            var settings = new JsonSerializerSettings();
            if (App.SpInvoker.Options.UseCamelCase)
                settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(_SpCallError, settings);
        }
    }

}


