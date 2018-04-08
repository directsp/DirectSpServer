using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectSp.AuthServer.Entities;
using DirectSp.Core.Entities;
using DirectSp.Core;

namespace DirectSp.AuthServer
{
    public static class AuthDB
    {
        public enum PasswordMatchType
        {
            None = 1,
            Password = 2,
            OneTimePassword = 3
        }

        public static SpInvoker SpInvoker { get { return App.SpInvoker; } }

        internal static async Task<Application> Application_Props(string clientId)
        {
            var spCall = new SpCall() { Method = nameof(Application_Props) };
            spCall.Params.Add("ClientId", clientId);
            var res = await SpInvoker.Invoke(spCall);
            var application = JsonConvert.DeserializeObject<Application>(JsonConvert.SerializeObject(res));
            application.ClientId = clientId;
            return application;
        }

        internal static async Task<Application> Application_GetByLogoutUrl(string postLogoutRedirectUri)
        {
            var spCall = new SpCall() { Method = nameof(Application_GetByLogoutUrl) };
            var ret = await SpInvoker.Invoke(spCall);
            return JsonConvert.DeserializeObject<Application>(JsonConvert.SerializeObject(ret));
        }

        //Return null if user not found or password not match
        internal static async Task<UserCredential> User_GetCredentialByLoginName(string loginName)
        {
            var spCall = new SpCall() { Method = nameof(User_GetCredentialByLoginName) };
            spCall.Params.Add("LoginName", loginName);
            var ret = await SpInvoker.Invoke(spCall);
            return JsonConvert.DeserializeObject<UserCredential>(JsonConvert.SerializeObject(ret));
        }

        internal static async Task<UserCredential> User_Credential(string userId)
        {
            var spCall = new SpCall() { Method = nameof(User_Credential) };
            spCall.Params.Add("UserId", userId);
            var ret = await SpInvoker.Invoke(spCall);
            return JsonConvert.DeserializeObject<UserCredential>(JsonConvert.SerializeObject(ret));
        }

        internal static async Task<SpCallResult> User_OnLogining(string userId, string clientId, IEnumerable<string> scopes, PasswordMatchType passwordMatchType, SpInvokeParams spInvokeParams)
        {
            var spCall = new SpCall() { Method = nameof(User_OnLogining) };
            spCall.Params.Add("UserId", userId);
            spCall.Params.Add("ClientId", clientId);
            spCall.Params.Add("Scopes", scopes.ToArray());
            spCall.Params.Add("PasswordMatchTypeId", passwordMatchType);
            return await SpInvoker.Invoke(spCall, spInvokeParams);
        }

        internal static async Task<SpCallResult> User_OnGranting(string userId, string clientId, IEnumerable<string> scopes, PasswordMatchType passwordMatchdType)
        {
            var spCall = new SpCall() { Method = nameof(User_OnGranting) };
            spCall.Params.Add("UserId", userId);
            spCall.Params.Add("ClientId", clientId);
            spCall.Params.Add("Scopes", scopes.ToArray());
            spCall.Params.Add("PasswordMatchTypeId", passwordMatchdType);
            return await SpInvoker.Invoke(spCall);
        }

        internal static async Task<SpCallResult> User_OnRefreshingToken(string userId, string clientId, IEnumerable<string> scopes)
        {
            var spCall = new SpCall() { Method = nameof(User_OnRefreshingToken) };
            spCall.Params.Add("UserId", userId);
            spCall.Params.Add("ClientId", clientId);
            spCall.Params.Add("Scopes", scopes.ToArray());
            return await SpInvoker.Invoke(spCall);
        }

        internal static async Task<IEnumerable<Application>> Application_ApplicationsByClientIds(IEnumerable<string> clientIds)
        {
            var spCall = new SpCall() { Method = nameof(Application_ApplicationsByClientIds) };
            spCall.Params.Add("ClientIds", clientIds.ToArray());
            var apiResult = await SpInvoker.Invoke(spCall);
            var applications = apiResult["Applications"];
            return JsonConvert.DeserializeObject<IEnumerable<Application>>(JsonConvert.SerializeObject(applications));
        }

        //Throw Exception if UserId is not exists
        internal static async Task<User> User_Props(string UserId)
        {
            var spCall = new SpCall() { Method = nameof(User_Props) };
            spCall.Params.Add("UserId", UserId);
            var ret = await SpInvoker.Invoke(spCall);

            var obj = JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(ret));
            if (obj.Gender == "1") obj.Gender = "male";
            if (obj.Gender == "2") obj.Gender = "female";
            obj.UserId = UserId;
            return obj;
        }

        internal static async Task<User> User_Login(string loginName, string password, string clientId, IEnumerable<string> scopes, SpInvokeParams spInvokeParams)
        {
            var userCredential = await User_GetCredentialByLoginName(loginName);
            var matchType = User_MatchPassword(userCredential, password);
            await User_OnLogining(userCredential.UserId, clientId, scopes, matchType, spInvokeParams);
            return await User_Props(userCredential.UserId);
        }

        internal static async Task<PasswordMatchType> User_MatchPassword(string userId, string password)
        {
            var userCredential = await User_Credential(userId);
            return User_MatchPassword(userCredential, password);
        }

        private static PasswordMatchType User_MatchPassword(UserCredential userCredential, string password)
        {
            //check OnTimePassword and expiration
            if (CheckPassword(password, userCredential.Password))
                return PasswordMatchType.Password;

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(userCredential.OneTimePassword) && password.Trim() == userCredential.OneTimePassword.Trim())
                return PasswordMatchType.OneTimePassword;

            return PasswordMatchType.None;
        }

        private static bool CheckPassword(string password, string passwordData)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordData))
                return false;

            var chunks = passwordData.Split('$');
            var algorithm = chunks[0];
            var iteration = int.Parse(chunks[1]);
            var salt = chunks[2];
            var hash = chunks[3];
            var reqByte = Convert.FromBase64String(hash).Length;

            var prf = KeyDerivationPrf.HMACSHA1;
            if (algorithm.ToLower() == "pbkdf2_sha256") prf = KeyDerivationPrf.HMACSHA256;
            else if (algorithm.ToLower() == "pbkdf2_sha512") prf = KeyDerivationPrf.HMACSHA512;

            var saltBuf = System.Text.Encoding.ASCII.GetBytes(salt);
            var newHash = KeyDerivation.Pbkdf2(password, saltBuf, prf, iteration, reqByte);

            var hashString = Convert.ToBase64String(newHash);
            return hashString == hash;
        }
    }
}