using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;

namespace DirectSp.Host.Auth
{
    public static class AppAuthExtension
    {
        public static IApplicationBuilder UseAppAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AppAuthentication>();
        }
    }
}
