using Microsoft.AspNetCore.Builder;

namespace DirectSp.Host
{
    public static class AppFilterExtension
    {
        public static IApplicationBuilder UseAppFilter(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AppFilter>();
        }
    }

}
