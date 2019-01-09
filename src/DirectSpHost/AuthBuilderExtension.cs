using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Host
{
    public static class AuthBuilderExtension
    {
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services)
        {
            var authOptions = App.HostSettings.Authentication;
            var cert = new X509Certificate2(authOptions.CertificateFile);
            var signingKey = new X509SecurityKey(cert);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = new Uri(authOptions.ValidIssuer).ToString(),
                    IssuerSigningKey = signingKey,
                    ValidAudiences = App.HostSettings.Authentication.ValidAudiences,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(authOptions.ClockSkew)
                };

            });
            return services;
        }
    }
}
