using Microsoft.AspNetCore.Builder;

namespace DirectSp
{
    public static class DirectSpFilterExtension
    {
        public static IApplicationBuilder UseDirectSpFilter(this IApplicationBuilder app, DirectSpHttpHandler directSpHttpHandler)
        {
            return app.UseMiddleware<DirectSpFilter>(directSpHttpHandler);
        }
    }

}
