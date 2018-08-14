using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using DirectSp.Core.InternalDb;

//Test: password flow => OK
//Test: client_credentials flow >= OK
//Test: implicit flow >= OK
//Test: authorization_flow >= OK
//Test: refresh_token >= OK
//Test: certificate >= OK
//Test: resource server >= OK
//Test: AsymetricSign >= OK
//Test: SymetricSign >= NO

namespace DirectSp.AuthServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
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
            //read app settings
            App.Configure(Configuration);

            //enable cross-origin
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("WWW-Authenticate", "DSP-AppVersion")
                    .SetPreflightMaxAge(TimeSpan.FromHours(24 * 30));
            }));


            // configure data protection
            services.AddDataProtection()
                .ProtectKeysWithCertificate(App.ServerCertificate)
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(App.AppSettings.KeysFolderPath))
                .SetApplicationName("DirectSpAuthServer");

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            // Add authentications.
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddDspAuthorizationServer()
            .AddDspAuthorizationClient();

            //Init MVC
            services.AddMvc().AddJsonOptions(options =>
            {
                if (App.SpInvoker.Options.UseCamelCase)
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Cors must configure before any Authorization to allow token request
            app.UseCors("CorsPolicy");

            //User Authentication Server and Client (Before Static Files and MVC)
            app.UseAppFilter(env); //WARNING: UseAppFilter MUST be called before UseAuthentication
            app.UseAuthentication();

            //Add System services
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
