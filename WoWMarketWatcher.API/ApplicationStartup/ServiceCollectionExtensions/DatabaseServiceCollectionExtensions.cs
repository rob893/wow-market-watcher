using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MySQLSettings>(config.GetSection(ConfigurationKeys.MySQL));

            var settings = config.GetSection(ConfigurationKeys.MySQL).Get<MySQLSettings>();

            services.AddDbContext<DataContext>(
                dbContextOptions =>
                {
                    dbContextOptions
                        .UseMySql(settings.DefaultConnection, ServerVersion.AutoDetect(settings.DefaultConnection), options =>
                        {
                            options.EnableRetryOnFailure();
                            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        });

                    if (settings.EnableDetailedErrors)
                    {
                        dbContextOptions.EnableDetailedErrors();
                    }

                    if (settings.EnableSensitiveDataLogging)
                    {
                        dbContextOptions.EnableSensitiveDataLogging();
                    }
                }
            );

            services.AddTransient<IDatabaseSeeder, DatabaseSeeder>();

            return services;
        }
    }
}