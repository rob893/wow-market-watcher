using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.Common.Extensions;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;

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
                p.WaitAndRetryAsync(5, (retryAttempt) => TimeSpan.FromMilliseconds(retryAttempt * 300), onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var sourceName = GetSourceName();
                    var correlationId = outcome.Result.RequestMessage?.Headers.GetOrGenerateCorrelationId() ?? Guid.NewGuid().ToString();

                    services.BuildServiceProvider().GetRequiredService<ILogger<BlizzardService>>()
                        .LogWarning(sourceName, correlationId, $"Request to {outcome.Result.RequestMessage?.RequestUri} failed with status {outcome.Result.StatusCode}. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                }));

            services.AddScoped<IBlizzardService, BlizzardService>();

            return services;
        }
    }
}