using DirectSpClientNet.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DirectSpClientNet
{
    public class DirectSpClient
    {
        public string[] authScopes { get; set; } = { OpenIdScope.profile.ToString(), OpenIdScope.offline_access.ToString() };
        public AuthType authType { get; set; } = AuthType.code;

        public Uri authEndpointUri { get; set; }
        public Uri tokenEndpointUri { get; set; }
        public Uri userinfoEndpointUri { get; set; }
        public Uri logoutEndpointUri { get; set; }
        public Uri resourceApiUri { get; set; }

        private Uri _authBaseUri;
        public Uri authBaseUri
        {
            get { return _authBaseUri; }
            set
            {
                _authBaseUri = value;
                authEndpointUri = new Uri(value, "/connect/authorize");
                tokenEndpointUri = new Uri(value, "/connect/token");
                userinfoEndpointUri = new Uri(value, "/connect/userinfo");
                logoutEndpointUri = new Uri(value, "/connect/logout");
            }
        }

        public string authHeader
        {
            get
            {
                return tokens != null ? tokens.token_type + ' ' + tokens.access_token : null;
            }
        }

        private AuthTokens _tokens;
        public AuthTokens tokens
        {
            get
            {
                return _tokens;
            }
            set
            {
                _tokens = value;
            }
        }

        public string username { get; private set; }
        public string clientId { get; set; }

        private void resetUser()
        {
            _tokens = null;
        }

        public async Task signInByPasswordGrant(string username, string password)
        {
            if (clientId == null) throw new ArgumentNullException("ClientId", "CliendId is not initialized");
            if (tokenEndpointUri == null) throw new ArgumentNullException("TokenEndpointUri", "TokenEndpointUri is not initialized");
            resetUser();

            //clear user info and tokens
            this.username = username;

            // create request param
            var requestContext = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "password" },
                { "username", username },
                { "password", password },
                { "client_id", clientId },
                { "scope", string.Join(" ", authScopes) },
            });

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(tokenEndpointUri, requestContext);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw DirectSpException.fromHttpResponse(response.StatusCode, responseContent);

            tokens = JsonConvert.DeserializeObject<AuthTokens>(responseContent);
        }

        public async Task<object> invoke(string method, object args, InvokeOptions invokeOptions = null)
        {
            var spCall = new SpCall()
            {
                args = args,
                method = method
            };

            return await invoke(spCall, invokeOptions);
        }

        private async Task<object> invoke(SpCall spCall, InvokeOptions invokeOptions = null)
        {
            var invokeParams = new InvokeParams()
            {
                spCall = spCall,
                invokeOptions = invokeOptions,
            };

            return await invoke(invokeParams);
        }

        private async Task<object> invoke(InvokeParams invokeParams)
        {
            if (invokeParams == null) throw new ArgumentNullException("invokeParam");
            if (invokeParams.spCall == null) throw new ArgumentNullException("invokeParams.spCall");
            if (string.IsNullOrWhiteSpace(invokeParams.spCall.method)) throw new ArgumentNullException("invokeParams.spCall.method");
            if (resourceApiUri == null) throw new ArgumentNullException("resourceApiUri");

            // set defaults
            if (invokeParams.invokeOptions == null) invokeParams.invokeOptions = new InvokeOptions();

            //call 
            var requestContent = new StringContent(JsonConvert.SerializeObject(invokeParams), Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
            var response = await httpClient.PostAsync(resourceApiUri, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            // check error
            if (!response.IsSuccessStatusCode)
                throw DirectSpException.fromHttpResponse(response.StatusCode, responseContent);

            return JsonConvert.DeserializeObject(responseContent);
        }
    }
}
