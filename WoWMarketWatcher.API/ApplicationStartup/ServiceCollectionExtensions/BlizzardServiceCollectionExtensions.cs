using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class BlizzardServiceCollectionExtensions
    {
        public static IServiceCollection AddBlizzardServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<BlizzardSettings>(config.GetSection("Blizzard"));

            var settings = config.GetSection("Blizzard").Get<BlizzardSettings>();

            services.AddHttpClient(nameof(BlizzardService), c =>
            {
                c.BaseAddress = new Uri(settings.BaseUrl);
            }).AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300)));

            services.AddScoped<BlizzardService>();

            return services;
        }
    }
}