using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                p.WaitAndRetryAsync(3, (retryAttempt) => TimeSpan.FromMilliseconds(retryAttempt * 300), onRetry: (outcome, _, retryAttempt) =>
                {
                    services.BuildServiceProvider().GetRequiredService<ILogger<BlizzardService>>().LogWarning($"Attempting retry for {outcome.Result.RequestMessage?.RequestUri}, then making retry {retryAttempt}.");
                }));

            services.AddScoped<IBlizzardService, BlizzardService>();

            return services;
        }
    }
}