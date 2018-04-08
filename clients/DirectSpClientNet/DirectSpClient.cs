using DirectSp.DirectSpClient.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.DirectSpClient
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

        public JObject userInfo { get; private set; }
        public string username { get; private set; }
        public string userId { get { return userInfo?["sub"]?.Value<string>(); } }
        public string userDisplayName { get { return userInfo?["name"]?.Value<string>() ?? username; } }
        public string clientId { get; set; }
        public bool isAuthorized { get { return _tokens != null; } }
        public bool isLogEnabled { get; set; }
        public bool autoUpdateUserInfo { get; set; } = true;

        private void resetUser()
        {
            _tokens = null;
        }

        public async Task signInByPasswordGrant(string username, string password)
        {
            if (clientId == null) throw new ArgumentNullException("ClientId");
            if (tokenEndpointUri == null) throw new ArgumentNullException("TokenEndpointUri");
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
            var responseContent = await getResponseString(response);

            //update token
            tokens = JsonConvert.DeserializeObject<AuthTokens>(responseContent);

            //update userinfo
            if (autoUpdateUserInfo)
                await updateUserInfo();
        }

        private async Task<string> getResponseString(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw DirectSpException.fromHttpResponse(response);
            return responseContent;
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

            //report
            if (isLogEnabled) Console.WriteLine($"\nDirectSp: invokeApi (Request)\ninvokeParams: {JsonConvert.SerializeObject(invokeParams)}");

            //call 
            var httpClient = new HttpClient();
            var requestContent = new StringContent(JsonConvert.SerializeObject(invokeParams), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
            var response = await httpClient.PostAsync(resourceApiUri, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            // check error
            if (!response.IsSuccessStatusCode)
                throw DirectSpException.fromHttpResponse(response.StatusCode, responseContent);

            var ret = JsonConvert.DeserializeObject(responseContent);
            if (isLogEnabled) Console.WriteLine($"\nDirectSp: invokeApi (Response)\ninvokeParams: {requestContent}\nResult: {ret}");
            return ret;
        }

        private async Task<bool> updateUserInfo(bool tryRefreshToken = true)
        {
            //return false if token not exists
            if (!isAuthorized)
                throw new DirectSpException() { errorName = "unauthorized", errorMessage = "Can not refresh token for unauthorized users" };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
            var response = await httpClient.GetAsync(userinfoEndpointUri);
            if (!response.IsSuccessStatusCode)
            {
                //refresh token
                if (isTokenExpired(response) && tryRefreshToken)
                {
                    await refreshToken();
                    return await updateUserInfo(false);
                }

                return false;
            }

            //updating userInfo
            var responseContent = await response.Content.ReadAsStringAsync();
            userInfo = JObject.Parse(responseContent);
            if (isLogEnabled)
                Console.WriteLine($"DirectSp: userInfo: {userInfo}");
            return true;
        }

        private async Task refreshToken()
        {
            //return false if token not exists
            if (tokens == null || tokens.refresh_token == null)
            {
                tokens = null;
                return;
            }

            //Refreshing token
            Console.WriteLine("DirectSp: Refreshing current token ...");

            //create request param
            var requestContext = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "grant_type", "refresh_token" },
                { "refresh_token", tokens.refresh_token },
                { "client_id", clientId }
            });

            // send request
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(tokenEndpointUri, requestContext);
            var responseContent = await getResponseString(response);

            //update token
            tokens = JsonConvert.DeserializeObject<AuthTokens>(responseContent);
        }

        private bool isTokenExpired(HttpResponseMessage httpResponse)
        {
            try
            {
                if (httpResponse.IsSuccessStatusCode)
                    return false;

                var ex = DirectSpException.fromHttpResponse(httpResponse);

                //check database AccessDeniedOrObjectNotExists error; it means token has been validated
                if (ex.errorName == "AccessDeniedOrObjectNotExists")
                    return false;

                //noinspection JSUnresolvedVariable
                if (ex.errorName == "invalid_grant")
                    return true;

                return ex.statusCode == System.Net.HttpStatusCode.Unauthorized;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
