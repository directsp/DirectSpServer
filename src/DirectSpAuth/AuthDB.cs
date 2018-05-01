using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectSp.AuthServer.Entities;
using DirectSp.Core.Entities;
using DirectSp.Core;
using Newtonsoft.Json.Linq;

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
            var res = await SpInvoker.Invoke(nameof(Application_Props), new { ClientId = clientId });
            var application = res.ConvertParams<Application>();
            application.ClientId = clientId;
            return application;
        }

        internal static async Task<Application> Application_GetByLogoutUrl(string logoutUrl)
        {
            var ret = await SpInvoker.Invoke(nameof(Application_GetByLogoutUrl), new { LogoutUrl = logoutUrl });
            return ret.ConvertParams<Application>();
        }

        //Return null if user not found or password not match
        internal static async Task<UserCredential> User_GetCredentialByLoginName(string loginName)
        {
            var ret = await SpInvoker.Invoke(nameof(User_GetCredentialByLoginName), new { LoginName = loginName });
            return ret.ConvertParams<UserCredential>();
        }

        internal static async Task<UserCredential> User_Credential(string userId)
        {
            var ret = await SpInvoker.Invoke(nameof(User_Credential), new { UserId = userId });
            return ret.ConvertParams<UserCredential>();
        }

        internal static async Task<SpCallResult> User_OnLogining(string userId, string clientId, IEnumerable<string> scopes, PasswordMatchType passwordMatchType, SpInvokeParams spInvokeParams)
        {
            var param = new
            {
                UserId = userId,
                ClientId = clientId,
                Scopes = scopes,
                PasswordMatchTypeId = passwordMatchType,
            };

            var ret = await SpInvoker.Invoke(nameof(User_OnLogining), param, spInvokeParams);
            return ret;
        }

        internal static async Task<SpCallResult> User_OnGranting(string userId, string clientId, IEnumerable<string> scopes, PasswordMatchType passwordMatchdType)
        {
            var param = new
            {
                UserId = userId,
                ClientId = clientId,
                Scopes = scopes.ToArray(),
                PasswordMatchTypeId = passwordMatchdType
            };

            var ret = await SpInvoker.Invoke(nameof(User_OnGranting), param);
            return ret;
        }

        internal static async Task<SpCallResult> User_OnRefreshingToken(string userId, string clientId, IEnumerable<string> scopes)
        {
            var param = new
            {
                UserId = userId,
                ClientId = clientId,
                Scopes = scopes.ToArray(),
            };

            var ret = await SpInvoker.Invoke(nameof(User_OnRefreshingToken), param);
            return ret;
        }

        internal static async Task<IEnumerable<Application>> Application_ApplicationsByClientIds(IEnumerable<string> clientIds)
        {
            var ret = await SpInvoker.Invoke(nameof(Application_ApplicationsByClientIds), new { ClientIds = clientIds.ToArray() });
            return ret.ConvertParam<IEnumerable<Application>>("Applications");
        }

        //Throw Exception if UserId is not exists
        internal static async Task<User> User_Props(string userId)
        {
            var result = await SpInvoker.Invoke(nameof(User_Props), new { UserId = userId });
            var ret = result.ConvertParams<User>();
            ret.UserId = userId;
            return ret;
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