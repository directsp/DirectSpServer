using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.AuthServer
{
    public static class AuthClientBuilderExtension
    {
        public static AuthenticationBuilder AddDspAuthorizationClient(this AuthenticationBuilder builder)
        {
            var cert = App.ServerCertificate;
            return builder.AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Audience = App.AppSettings.ServerAudience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = new Uri(App.AppSettings.ServerIssuerUri).ToString(),
                    IssuerSigningKey = new X509SecurityKey(cert),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.FromSeconds(App.AppSettings.ClockSkew)
                };
            });
        }
    }
}
