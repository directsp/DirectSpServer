using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectSp.Host
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

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
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddAppAuthentication();

            //Init MVC
            services.AddMvc().AddJsonOptions(options =>
            {
                if (App.SpInvoker.Options.UseCamelCase)
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
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Cors must configure before any Authorization to allow token request
            if (App.AppSettings.EnableCors)
                app.UseCors("CorsPolicy");

            //User Authentication Server and Client (Before Static Files and MVC)
            app.UseAppFilter(env); //WARNING: UseAppFilter MUST be called before UseAuthentication
            app.UseAuthentication();

            //Add System services
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            loggerFactory.AddLog4Net(); // << Add this line
            app.UseMvc();
        }
    }
}
