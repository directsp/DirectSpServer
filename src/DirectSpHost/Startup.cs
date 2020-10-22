using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using DirectSp.Host.Auth;

namespace DirectSp.Host
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //read settings
            App.Configure(Configuration);

            //enable cross-origin; MUST before anything
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    //.AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate", "DSP-AppVersion")
                    .SetPreflightMaxAge(TimeSpan.FromHours(24 * 30));
            }));


            // Add framework services.
            if (App.AuthProviderSettingsArray.Length > 0)
                services.AddAppAuthentication(App.AuthProviderSettingsArray);

            //Init MVC
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                if (App.DirectSpInvoker.UseCamelCase)
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });

            // Compression
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
                options.MimeTypes = new[] { "text/csv" };
                options.EnableForHttps = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Cors must configure before any Authorization to allow token request
            if (App.HostSettings.EnableCors)
                app.UseCors("CorsPolicy");

            //before UseAuthentication
            app.UseMiddleware<AppFilter>();

            //before UseAuthentication
            if (App.AuthProviderSettingsArray.Length > 0)
                app.UseAppAuthentication(App.AuthProviderSettingsArray);

            app.UseDirectSpFilter(new DirectSpHttpHandler(App.DirectSpInvoker, "api"));

            //Add System services
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
