using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var settingsSection = config.GetSection(ConfigurationKeys.Swagger);
            var settings = settingsSection.Get<SwaggerSettings>();

            services.Configure<SwaggerSettings>(settingsSection);

            services.AddSwaggerGen(options =>
            {
                foreach (var version in settings.SupportedApiVersions)
                {
                    options.SwaggerDoc(
                        version,
                        new OpenApiInfo
                        {
                            Version = version,
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
                }

                // Remove version parameter from ui
                options.OperationFilter<RemoveVersionParameterFilter>();
                // Replace {version} with actual version in routes in swagger doc
                options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
                options.CustomSchemaIds(id => id.FullName);

                // Add the security token option to swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                options.IncludeXmlComments(xmlPath);
                options.SwaggerGeneratorOptions.DescribeAllParametersInCamelCase = true;

                //options.Cus
            });

            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }

        private class RemoveVersionParameterFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");

                if (versionParameter == null)
                {
                    return;
                }

                operation.Parameters.Remove(versionParameter);
            }
        }

        private class ReplaceVersionWithExactValueInPathFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var paths = new OpenApiPaths();
                foreach (var path in swaggerDoc.Paths)
                {
                    paths.Add(path.Key.Replace("v{version}", swaggerDoc.Info.Version), path.Value);
                }
                swaggerDoc.Paths = paths;
            }
        }
    }
}