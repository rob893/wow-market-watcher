using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Core;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class HealthCheckServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHealthChecks()
                .AddCheck<VersionHealthCheck>(name: nameof(VersionHealthCheck), failureStatus: HealthStatus.Unhealthy)
                .AddCheck<BlizzardHealthCheck>(
                    name: nameof(BlizzardHealthCheck),
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckTags.Dependency])
                .AddHangfire(
                    options =>
                    {
                        options.MinimumAvailableServers = 1;
                    },
                    name: "Hangfire",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckTags.Hangfire, HealthCheckTags.Dependency])
                .AddCheck<AuctionTimeSeriesJobHealthCheck>(
                    name: nameof(AuctionTimeSeriesJobHealthCheck),
                    failureStatus: HealthStatus.Unhealthy,
                    tags: [HealthCheckTags.Database, HealthCheckTags.Dependency, HealthCheckTags.Hangfire]);

            return services;
        }
    }
}