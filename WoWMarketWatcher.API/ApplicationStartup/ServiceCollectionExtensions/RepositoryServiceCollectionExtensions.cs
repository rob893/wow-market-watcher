using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.Data.Repositories;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class RepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Interface => concrete implementation
            services.AddScoped<UserRepository>()
                .AddScoped<WoWItemRepository>()
                .AddScoped<RealmRepository>()
                .AddScoped<WatchListRepository>()
                .AddScoped<AuctionTimeSeriesRepository>()
                .AddScoped<ConnectedRealmRepository>();

            return services;
        }
    }
}