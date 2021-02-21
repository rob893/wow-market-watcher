using System;
using System.Drawing;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.JobsLogger;
using Hangfire.Logging;
using Hangfire.Storage.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.BackgroundJobs;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class HangfireServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration config)
        {
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 10 });

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseHeartbeatPage(TimeSpan.FromSeconds(1.5))
                .UseJobsLogger(new JobsLoggerOptions
                {
                    LogCriticalColor = Color.DarkRed,
                    LogErrorColor = Color.Red,
                    LogWarningColor = Color.DarkOrange,
                    LogInformationColor = Color.DarkGreen,
                    LogDebugColor = Color.Gray
                })
                .UseColouredConsoleLogProvider(LogLevel.Warn)
                .UseStorage(new MySqlStorage(config["Hangfire:DatabaseConnection"], new MySqlStorageOptions
                {
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "Hangfire"
                }))
            );

            services.AddTransient<PullAuctionDataBackgroundJob>();

            services.AddHangfireServer();

            return services;
        }
    }
}