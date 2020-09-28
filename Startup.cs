using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Evolution.Internet.Filters;
using Evolution.Internet.Logic;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace Evolution.Internet
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(Configuration)
              .Enrich.FromLogContext()
              .Enrich.WithMachineName()
              .CreateLogger();

            Log.Logger.Debug("Startup");
            try
            {

                // Initialize evolution asposer
                Asposer.ModuleInitializer.Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error in startup.ctor");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ApiExceptionFilter>();
            services.ConfigurePOCO<AppSettings>(Configuration.GetSection("AppSettings"));

            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    opts =>
                    {
                        opts
                        // TODO: specify origins
                        .AllowAnyOrigin()

                        .AllowAnyMethod()
                        //.WithMethods("GET")
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });

            });

            // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            services.AddMvcCore().AddVersionedApiExplorer(o => o.GroupNameFormat = "'v'VVV");

            services.AddMvc(opt =>
            {
                opt.RespectBrowserAcceptHeader = true;
                opt.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            })
            .AddJsonOptions(opt =>
            {
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                opt.SerializerSettings.Formatting = Formatting.Indented;
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opt.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new MediaTypeApiVersionReader();
            });
            services.AddSwaggerGen(options =>
            {
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                var serviceProvider = services.BuildServiceProvider();
                var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
                // add a swagger document for each discovered API version
                // note: you might choose to skip or document deprecated API versions differently
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description, env));
                }

                // add a custom operation filter which documents the implicit API version parameter
                //options.OperationFilter<ImplicitApiVersionParameter>();

                // integrate xml comments
                options.IncludeXmlComments(XmlCommentsFilePath);
            });

            services.AddScoped(factory =>
            {
                var connectionName = Configuration.GetSection("AppSettings:ConnectionName").Value;
                var connectionString = Configuration.GetConnectionString(connectionName);
                return new DbContext(connectionString);
            });


            // See: https://lostechies.com/jimmybogard/2016/07/20/integrating-automapper-with-asp-net-core-di/
            services.AddAutoMapper(typeof(AppSettings));

            services.AddMediatR(typeof(AppSettings));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="provider"></param>
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApiVersionDescriptionProvider provider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // global policy - assign here or on each controller
            app.UseCors("CorsPolicy");

            // Custom default redirect to index.html
            app.UseDefaultRedirect();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
              //options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
              // build a swagger endpoint for each discovered API version
              foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });

            app.UseMvc();
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = AppContext.BaseDirectory;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }

        static Info CreateInfoForApiVersion(ApiVersionDescription description, IHostingEnvironment env)
        {
            //var descriptionPath = Path.Combine(env.ContentRootPath, "README.md");

            var info = new Info()
            {
                Title = $"Evolution Internet Web API",
                Version = description.ApiVersion.ToString(),

                Description = "",//File.ReadAllText(descriptionPath),
                //Contact = new Contact() { Name = "Tommy Blomskog", Email = "tommy.blomskog@essvision.se" },
                //TermsOfService = "Shareware",
                //License = new License() { Name = "MIT", Url = "https://opensource.org/licenses/MIT" }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
