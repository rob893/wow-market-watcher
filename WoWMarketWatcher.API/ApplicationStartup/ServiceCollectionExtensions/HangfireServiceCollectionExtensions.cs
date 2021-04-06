using System;
using System.Drawing;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.JobsLogger;
using Hangfire.Logging;
using Hangfire.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using WoWMarketWatcher.API.BackgroundJobs;
using WoWMarketWatcher.API.Constants;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class HangfireServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration config)
        {
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 10 });

            var mySqlOptions = new MySqlStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(15),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 50000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "Hangfire"
            };

            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseHeartbeatPage(TimeSpan.FromSeconds(1.5))
                .UseJobsLogger(new JobsLoggerOptions
                {
                    LogLevel = LogLevelFromString(config[ConfigurationKeys.HangfireJobsLoggerLevel]),
                    LogCriticalColor = Color.DarkRed,
                    LogErrorColor = Color.Red,
                    LogWarningColor = Color.DarkOrange,
                    LogInformationColor = Color.DarkGreen,
                    LogDebugColor = Color.Gray
                })
                .UseColouredConsoleLogProvider(LogLevel.Warn)
                // Can't use tags yet. Issue with Pomelo ef core MySQL connector. Also won't work with using Hangfire.Storage.MySql;
                // .UseTagsWithMySql(new TagsOptions { TagsListStyle = TagsListStyle.Dropdown }, mySqlOptions)
                .UseStorage(new MySqlStorage(GetHangfireMySqlConnectionString(config[ConfigurationKeys.HangfireDatabaseName], config[ConfigurationKeys.HangfireDatabaseConnection]), mySqlOptions))
            );

            services.AddTransient<PullAuctionDataBackgroundJob>();

            services.AddHangfireServer();

            return services;
        }

        private static string GetHangfireMySqlConnectionString(string hangfireDbName, string dbConnectionString)
        {

            using var connection = new MySqlConnection(dbConnectionString);
            connection.Open();

            using var command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{hangfireDbName}`", connection);
            command.ExecuteNonQuery();

            connection.Close();

            return $"{dbConnectionString.TrimEnd(';')}; Database={hangfireDbName};";
        }
    }
}