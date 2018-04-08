using AspNet.Security.OpenIdConnect.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DirectSp.AuthServer
{
    public static class AuthUtil
    {
        public static string GetUserId(ClaimsPrincipal user)
        {
            var claim = user.Claims.FirstOrDefault(x => x.Type == OpenIdConnectConstants.Claims.Subject || x.Type == ClaimTypes.NameIdentifier);
            return claim?.Value;
        }

    }
}
