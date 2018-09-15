using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using afs.jwt.abstractions;
using afs.jwt.issuer;
using afs.jwt.validator;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Examples;
using Swashbuckle.AspNetCore.Swagger;

namespace afs.jwt.example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls("http://*:8088")
                .UseStartup<Startup>();
    }

    public static class SampleJwtModule
    {
        public static IServiceCollection RegisterJwt(this IServiceCollection services, IConfiguration config)
        {
            // add http accessor to access http request data in mediator pipeline
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // options
            services.Configure<JwtIssuerOptions>(config);
            services.Configure<JwtKeyOptions>(config);

            // symmetric key store for both issuer and validator
            services.AddSingleton<ITokenKeyStore, MockJwtSymmetricKeyStore>();

            // issuer with asymmetric key store
            services.AddSingleton<ITokenKeyStore, JwtPrivateKeyStore>();
            services.AddTransient<ITokenIssuer, JwtTokenIssuer>();

            // validator with asymmetric key store
            services.AddSingleton<ITokenKeyStore, JwtPublicKeyStore>();
            services.AddTransient<ITokenValidator, JwtTokenValidator>();

            return services;
        }
    }

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) { Configuration = configuration; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // register MVC with CORS services
            services.AddCors();
            services.AddMvcCore()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddCors()
                .AddApiExplorer() // for swagger gen
                .AddJsonOptions(o => { o.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; })
                .AddJsonFormatters()
                .AddFormatterMappings()
                .AddDataAnnotations();

            // JWT validator and issuer
            services.RegisterJwt(Configuration);

            // register Swagger Generator
            services.AddSwaggerGen(Configuration, "afs.jwt.example", "v1");

            // register Logging
            services.AddNLog(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable CORS first
            app.UseCors(cfg => cfg.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // Enable Swagger engine and Swagger UI
            app.ConfigurewaggerUI("afs.jwt.example", "v1");

            // Enable MVC
            app.UseMvc();
            app.UseStaticFiles();
            app.UseDefaultFiles();

            // Unhadled exceptions
            //app.UseExceptionHandler(b => b.Run(async c => await c.WriteInternalError()));

            // https
            app.UseHsts();
            app.UseHttpsRedirection();
        }
    }

    public static class SwaggerRegistrationExtensions
    {
        public static IServiceCollection AddSwaggerGen(this IServiceCollection services, IConfiguration config, string title, string version = "v1")
            => services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new Info { Title = title, Version = version });

                c.OperationFilter<ExamplesOperationFilter>();
                c.OperationFilter<DescriptionOperationFilter>();
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                c.MapType<Guid>(() => new Schema { Type = "string", Format = "uuid" });
                c.MapType<Guid?>(() => new Schema { Type = "string", Format = "uuid" });
                c.DescribeAllEnumsAsStrings();

                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();

                // add all XMLdoc files
                var location = Assembly.GetEntryAssembly().Location;
                var directory = Path.GetDirectoryName(location);
                foreach (var xmlDocPath in Directory.EnumerateFiles(directory, "*.xml", SearchOption.TopDirectoryOnly))
                    c.IncludeXmlComments(xmlDocPath);

                c.AddSecurityDefinition("Basic", new ApiKeyScheme
                {
                    Description = "HTTP Basic Auth header. Example: \"Authorization: Basic YWRtaW46UXdlcnR5ITIz\" - Base64(admin:Qwerty!23) ",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Basic", new string[] { }}
                });
            });

        public static void ConfigurewaggerUI(this IApplicationBuilder app, string title, string version = "v1")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{version}/swagger.json", title);
                c.ShowExtensions();
                c.DisplayRequestDuration();
                c.DisplayOperationId();
                c.DocExpansion(DocExpansion.List);
            });
        }
    }

    public static class NLogRegistrationExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddLogging(b =>
            {
                b.SetMinimumLevel(LogLevel.Trace);
                b.ClearProviders();
                b.AddConsole();
                b.AddDebug();
            });

            return services;
        }
    }
}
