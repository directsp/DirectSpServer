using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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
                    .AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate", "DSP-AppVersion")
                    .SetPreflightMaxAge(TimeSpan.FromHours(24 * 30));
            }));

            // Add framework services.
            if (App.HostSettings.Authentication != null)
                services.AddAppAuthentication(App.HostSettings.Authentication);

            //Init MVC
            services.AddMvc().AddJsonOptions(options =>
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment() || true)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Cors must configure before any Authorization to allow token request
            if (App.HostSettings.EnableCors)
                app.UseCors("CorsPolicy");

            //User Authentication Server and Client (Before Static Files and MVC)
            app.UseAppFilter(env); //WARNING: UseAppFilter MUST be called before UseAuthentication
            if (App.HostSettings.Authentication != null)
                app.UseAuthentication();
            app.UseDirectSpFilter(new DirectSpHttpHandler(App.DirectSpInvoker, "api"));

            //Add System services
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
