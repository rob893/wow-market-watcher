using AspNetCoreRateLimit;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions;
using WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Middleware;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.ApplicationStartup
{
    public sealed class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the DI container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllerServices()
                .AddLogging()
                .AddRateLimitingServices(this.configuration)
                .AddApplicationInsightsTelemetry()
                .AddDatabaseServices(this.configuration)
                .AddAuthenticationServices(this.configuration)
                .AddIdentityServices()
                .AddRepositoryServices()
                .AddSingleton<Counter>()
                .AddSingleton<ITelemetryInitializer, ApplicationInsightsTelemetryInitializer>()
                .AddScoped<ICorrelationIdService, CorrelationIdService>()
                .AddScoped<IAlertService, AlertService>()
                .AddHangfireServices(this.configuration)
                .AddSwaggerServices(this.configuration)
                .AddEmailServices(this.configuration)
                .AddAutoMapper(typeof(Startup))
                .AddHealthCheckServices()
                .AddBlizzardServices(this.configuration)
                .AddMemoryCache()
                .AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IRecurringJobManager recurringJobs, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIpRateLimiting()
                .UseExceptionHandler(builder => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>())
                .UseHsts()
                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.All
                })
                .UseMiddleware<PathBaseRewriterMiddleware>()
                .UseMiddleware<CorrelationIdMiddleware>()
                .UseRouting()
                .UseCors(header =>
                    header.WithOrigins(this.configuration.GetSection(ConfigurationKeys.CorsAllowedOrigins).Get<string[]>() ?? new[] { "*" })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders(this.configuration.GetSection(ConfigurationKeys.CorsExposedHeaders).Get<string[]>() ?? new[] { AppHeaderNames.TokenExpired, AppHeaderNames.CorrelationId })
                )
                .UseAuthentication()
                .UseAuthorization()
                .UseAndConfigureSwagger(this.configuration)
                .UseAndConfigureHangfire(recurringJobs, this.configuration)
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks(ApplicationSettings.HealthCheckEndpoint, new HealthCheckOptions()
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    endpoints.MapHealthChecks(ApplicationSettings.LivenessHealthCheckEndpoint, new HealthCheckOptions()
                    {
                        Predicate = (check) => !check.Tags.Contains(HealthCheckTags.Dependency),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    endpoints.MapControllers();
                });
        }
    }
}