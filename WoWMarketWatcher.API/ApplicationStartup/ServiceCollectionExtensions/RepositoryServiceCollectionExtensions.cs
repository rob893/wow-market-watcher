using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.Data.Repositories;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class RepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>()
                .AddScoped<IWoWItemRepository, WoWItemRepository>()
                .AddScoped<IRealmRepository, RealmRepository>()
                .AddScoped<IWatchListRepository, WatchListRepository>()
                .AddScoped<IAuctionTimeSeriesRepository, AuctionTimeSeriesRepository>()
                .AddScoped<IConnectedRealmRepository, ConnectedRealmRepository>();

            return services;
        }
    }
}