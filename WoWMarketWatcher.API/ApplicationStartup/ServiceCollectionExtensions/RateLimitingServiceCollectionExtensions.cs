using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.Constants;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class RateLimitingServiceCollectionExtensions
    {
        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<IpRateLimitOptions>(config.GetSection(ConfigurationKeys.IpRateLimiting));

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }
    }
}