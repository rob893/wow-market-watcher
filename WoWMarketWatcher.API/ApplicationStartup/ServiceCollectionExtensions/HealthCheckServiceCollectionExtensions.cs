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
                .AddHangfire(
                    options =>
                    {
                        options.MinimumAvailableServers = 1;
                    },
                    name: "Hangfire",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { HealthCheckTags.Hangfire, HealthCheckTags.Dependency })
                .AddCheck<AuctionTimeSeriesJobHealthCheck>(
                    name: "AuctionTimeSeriesJob",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { HealthCheckTags.Database, HealthCheckTags.Dependency, HealthCheckTags.Hangfire });

            return services;
        }
    }
}