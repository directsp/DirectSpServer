using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //enable cross-origin; MUST before anything
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate", "DSP-AppVersion")
                    .SetPreflightMaxAge(TimeSpan.FromHours(24 * 30));
            }));

            // Add framework services.
            services.AddAppAuthentication();

            //Init MVC
            services.AddMvc().AddJsonOptions(options =>
            {
                if (App.Invoker.UseCamelCase)
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Cors must configure before any Authorization to allow token request
            if (App.HostSettings.EnableCors)
                app.UseCors("CorsPolicy");

            //User Authentication Server and Client (Before Static Files and MVC)
            app.UseAppFilter(env); //WARNING: UseAppFilter MUST be called before UseAuthentication
            app.UseAuthentication();

            //Add System services
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
