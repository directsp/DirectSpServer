using DirectSp.Client.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DirectSp.Client
{
    public class DirectSpClient
    {
        private const long refreshClockSkew = 60;

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

        public JObject accessTokenInfo { get; private set; }
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
                accessTokenInfo = value?.parseAccessToken();
            }
        }

        public JObject userInfo { get; private set; }
        public string username { get; private set; }
        public string userId { get { return accessTokenInfo?["sub"]?.Value<string>(); } }
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

        public async Task<JObject> invoke(string method, object param, InvokeOptions invokeOptions = null)
        {
            var spCall = new SpCall()
            {
                param = param,
                method = method
            };

            return await invoke(spCall, invokeOptions);
        }

        public async Task<JObject> invoke(SpCall spCall, InvokeOptions invokeOptions = null)
        {
            var invokeParams = new InvokeParams()
            {
                spCall = spCall,
                invokeOptions = invokeOptions,
            };

            return await invoke(invokeParams);
        }

        public async Task<JObject> invoke(SpCall[] spCalls, InvokeOptions invokeOptions = null)
        {
            // send requests to server
            var invokeParamsBatch = new InvokeParamsBatch()
            {
                spCalls = spCalls,
                invokeOptions = invokeOptions ?? new InvokeOptions()
            };

            //append request id
            invokeParamsBatch.invokeOptions.RequestId = Guid.NewGuid().ToString();

            var content = JsonConvert.SerializeObject(invokeParamsBatch);
            return await invokePost("invokeBatch", content);
        }

        private async Task<JObject> invoke(InvokeParams invokeParams)
        {
            // validate arguments
            if (invokeParams == null) throw new ArgumentNullException("invokeParam");
            if (invokeParams.spCall == null) throw new ArgumentNullException("invokeParams.spCall");
            if (string.IsNullOrWhiteSpace(invokeParams.spCall.method)) throw new ArgumentNullException("invokeParams.spCall.method");

            // set defaults
            if (invokeParams.invokeOptions == null)
                invokeParams.invokeOptions = new InvokeOptions();

            //append request id
            invokeParams.invokeOptions.RequestId = Guid.NewGuid().ToString();

            var content = JsonConvert.SerializeObject(invokeParams);
            return await invokePost(invokeParams.spCall.method, content);
        }

        private async Task<JObject> invokePost(string methodName, string content)
        {
            //validate
            if (resourceApiUri == null) throw new ArgumentNullException("resourceApiUri");

            // refreshing token
            await refreshToken();

            // report
            if (isLogEnabled)
                Console.WriteLine($"\nDirectSp: invokeApi (Request) - {methodName}\ninvokeParams: {content}");

            // method uri
            var methodUri = new UriBuilder(resourceApiUri);
            methodUri.Path = methodUri.Path.Trim('/') + "/" + methodName;

            // call 
            var httpClient = new HttpClient();
            var requestContent = new StringContent(content, Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
            var response = await httpClient.PostAsync(methodUri.Uri, requestContent);
            var responseContent = await getResponseString(response);

            var ret = JsonConvert.DeserializeObject<JObject>(responseContent);
            if (isLogEnabled)
                Console.WriteLine($"\nDirectSp: invokeApi (Response) - {methodName}\ninvokeParams: {requestContent}\nResult: {ret}");
            return ret;
        }


        private async Task<bool> updateUserInfo()
        {
            //return false if token not exists
            if (!isAuthorized)
                throw new DirectSpException() { errorName = "unauthorized", errorMessage = "Can not refresh token for unauthorized users" };

            // refresh token
            await refreshToken();

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("authorization", authHeader);
            var response = await httpClient.GetAsync(userinfoEndpointUri);
            var responseContent = await getResponseString(response);

            //updating userInfo
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

            // check token expiration time
            var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var t = (DateTime.Now.ToUniversalTime() - st);
            var tokenCreatedUniversalTime = accessTokenInfo["exp"].Value<int>();
            if (tokenCreatedUniversalTime - t.TotalSeconds > refreshClockSkew)
                return;

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
