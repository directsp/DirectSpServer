using DirectSp.Host.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography.X509Certificates;

namespace DirectSp.Host
{
    public static class AuthBuilderExtension
    {
        internal static IServiceCollection AddAppAuthentication(this IServiceCollection services, AuthSettings authSettings)
        {
            var cert = new X509Certificate2(authSettings.CertificateFile);
            var signingKey = new X509SecurityKey(cert);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = new Uri(authSettings.ValidIssuer).ToString(),
                    IssuerSigningKey = signingKey,
                    ValidAudiences = authSettings.ValidAudiences,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(authSettings.ClockSkew)
                };

            });
            return services;
        }
    }
}
