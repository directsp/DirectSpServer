using AspNet.Security.OpenIdConnect.Primitives;
using System.Linq;
using System.Security.Claims;

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
