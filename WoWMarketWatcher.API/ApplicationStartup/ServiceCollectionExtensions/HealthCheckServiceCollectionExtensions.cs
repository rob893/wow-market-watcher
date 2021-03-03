using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class HealthCheckServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<DataContext>(
                    name: "Database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { HealthCheckTags.Database, HealthCheckTags.Dependency })
                .AddHangfire(
                    options =>
                    {
                        options.MinimumAvailableServers = 1;
                    },
                    name: "Hangfire",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { HealthCheckTags.Hangfire, HealthCheckTags.Dependency });

            return services;
        }
    }
}