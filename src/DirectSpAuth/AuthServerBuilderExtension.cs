using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.AuthServer
{
    public static class AuthServerBuilderExtension
    {
        public static AuthenticationBuilder AddDspAuthorizationServer(this AuthenticationBuilder builder)
        {
            builder.AddOpenIdConnectServer(options =>
            {
                options.AccessTokenHandler = new JwtSecurityTokenHandler();
                options.Provider = new AuthServerProvider();
                options.AuthorizationEndpointPath = "/connect/authorize"; //don't forget to sync it with AuthorizationController
                options.TokenEndpointPath = "/connect/token";
                options.UserinfoEndpointPath = "/connect/userinfo";
                options.LogoutEndpointPath = "/connect/logout";
                options.SigningCredentials.AddCertificate(App.ServerCertificate);
                options.AllowInsecureHttp = App.AppSettings.AllowInsecureHttp;
                options.ApplicationCanDisplayErrors = true;
                options.AccessTokenLifetime = TimeSpan.FromSeconds(App.AppSettings.AccessTokenLifetime);
                options.RefreshTokenLifetime = TimeSpan.FromSeconds(App.AppSettings.RefreshTokenLifetime);
                options.Issuer = new Uri(App.AppSettings.ServerIssuerUri);
            });

            return builder;
        }
    }
}
