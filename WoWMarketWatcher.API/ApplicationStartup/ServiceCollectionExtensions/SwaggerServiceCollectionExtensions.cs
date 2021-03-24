using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.Common.Extensions;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SwaggerSettings>(config.GetSection(ConfigurationKeys.Swagger));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Version = "v1",
                        Title = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName,
                        Description = $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} - {config.GetEnvironment()} ({Assembly.GetExecutingAssembly().GetName().Version})",
                        License = new OpenApiLicense
                        {
                            Name = "Hangfire Dashboard",
                            Url = new Uri(config[ConfigurationKeys.SwaggerHangfireEndpoint])
                        },
                        Contact = new OpenApiContact
                        {
                            Name = "Health Checks UI",
                            Url = new Uri("https://rwherber.com/health-checker/healthchecks-ui")
                        }
                    });

                // Add the security token option to swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;
            });

            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }
    }
}