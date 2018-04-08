using DirectSp.AuthServer;
using DirectSp.AuthServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectSp.AuthServer
{
    public static class ScopeManager
    {
        private static string[] OpenIdScopes { get; set; } = { "address", "email", "offline_access", "openid", "phone", "profile", "national_number" };
        public const string OpenIdClientId = "openid_a50225f017fb46139b9780e987f0baea";

        public static async Task<IEnumerable<Application>> Parse(string clientId, IEnumerable<string> scopes)
        {
            // get all client scopes
            var newScopes = new List<string>(scopes)
            {
                OpenIdClientId,
                clientId
            };
            var ret = await AuthDB.Application_ApplicationsByClientIds(newScopes);

            //filter openId scope
            var openIdApp = ret.FirstOrDefault(x => x.ClientId == OpenIdClientId);
            if (openIdApp != null)
                openIdApp.Scopes = openIdApp.Scopes.Where(x => scopes.FirstOrDefault(y => y == x.ScopeName) != null).ToArray();

            return ret;
        }

        public static IEnumerable<string> GetScopes(IEnumerable<Application> applications)
        {
            var ret = new List<string>();
            foreach (var item in applications)
            {
                var isOpenId = item.ClientId == OpenIdClientId;
                ret.AddRange(item.Scopes.Select(x => isOpenId ? x.ScopeName : x.ScopeId.ToString()));
            }

            return ret;
        }

        public static IEnumerable<string> GetAudiences(IEnumerable<Application> applications)
        {
            return applications
                .Where(x => x.ClientId != OpenIdClientId)
                .Select(x => x.ClientId);
        }
    }
}
