using DirectSp.Host.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DirectSp.Host.Auth
{
    public class AppAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, ClaimsPrincipal> _tokenCache = new ConcurrentDictionary<string, ClaimsPrincipal>();
        private AuthProviderSettings[] AuthProviderSettings => App.AuthProviderSettings;

        public AppAuthentication(RequestDelegate next)
        {
            _next = next;
        }

        private DateTime _lastCleanUpTime = DateTime.MinValue;
        private void CleanupCache()
        {
            if ((DateTime.Now - _lastCleanUpTime).TotalMilliseconds > 15)
                return;
            _lastCleanUpTime = DateTime.Now;

            foreach (var item in _tokenCache.ToArray())
            {
                var jwtExpValue = long.Parse(item.Value.Claims.FirstOrDefault(x => x.Type == "exp").Value);
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(jwtExpValue).DateTime;
                if (DateTime.Now > expirationTime)
                    _tokenCache.TryRemove(item.Key, out _);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            //Remove Expired Tokens
            CleanupCache();

            var authHeader = context.Request.Headers["authorization"].ToString();
            if (authHeader != null &&
                authHeader.Length > 7 &&
                authHeader.Substring(0, 7).Equals("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var tokenString = authHeader.Substring(7);
                var token = new JwtSecurityTokenHandler().ReadToken(tokenString);
                
                // check in cache
                if ( _tokenCache.TryGetValue(tokenString, out ClaimsPrincipal principal))
                {
                    if (DateTime.Now > token.ValidFrom && DateTime.Now < token.ValidTo)
                    {
                        context.User = principal;
                        await _next(context);
                    }
                    else
                    {
                        _tokenCache.TryRemove(tokenString, out _);
                    }
               }

                // find scheme
                var authProviderSettings = AuthProviderSettings.FirstOrDefault(x => x.Issuers.Contains(token.Issuer));
                if (authProviderSettings != null)
                {
                    // create new ticket
                    var result = await context.AuthenticateAsync(authProviderSettings.Name);
                    context.User = result.Principal;
                    _tokenCache.TryAdd(tokenString, result.Principal);
                }
            }

            await _next(context);
        }
    }

}
