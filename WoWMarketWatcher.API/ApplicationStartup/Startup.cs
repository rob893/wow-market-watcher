using WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.HttpOverrides;
using WoWMarketWatcher.API.Middleware;
using WoWMarketWatcher.API.Core;
using Hangfire;
using WoWMarketWatcher.Common.Constants;

namespace WoWMarketWatcher.API.ApplicationStartup
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllerServices()
                .AddLogging()
                .AddApplicationInsightsTelemetry()
                .AddDatabaseServices(this.Configuration)
                .AddAuthenticationServices(this.Configuration)
                .AddIdentityServices()
                .AddRepositoryServices()
                .AddSingleton<Counter>()
                .AddHangfireServices(this.Configuration)
                .AddSwaggerServices(this.Configuration)
                .AddAutoMapper(typeof(Startup))
                .AddHealthCheckServices()
                .AddBlizzardServices(this.Configuration)
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

            app.UseExceptionHandler(builder => builder.UseMiddleware<GlobalExceptionHandlerMiddleware>())
                .UseHsts()
                // .UseHttpsRedirection()
                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.All
                })
                .UseMiddleware<PathBaseRewriterMiddleware>()
                .UseRouting()
                .UseCors(header =>
                    header.WithOrigins(this.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders(this.Configuration.GetSection("Cors:ExposedHeaders").Get<string[]>() ?? new[] { AppHeaderNames.TokenExpired, AppHeaderNames.CorrelationId })
                )
                .UseAuthentication()
                .UseAuthorization()
                .UseAndConfigureSwagger(this.Configuration)
                .UseAndConfigureHangfire(recurringJobs, this.Configuration)
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    endpoints.MapControllers();
                });
        }
    }
}
