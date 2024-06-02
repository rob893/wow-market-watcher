using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class BlizzardServiceCollectionExtensions
    {
        public static IServiceCollection AddBlizzardServices(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            services.Configure<BlizzardSettings>(config.GetSection(ConfigurationKeys.Blizzard));

            var settings = config.GetSection(ConfigurationKeys.Blizzard).Get<BlizzardSettings>();

            services.AddHttpClient(nameof(BlizzardService), c =>
            {
                c.BaseAddress = settings.BaseUrl;
            })
                .AddPolicyHandler((IServiceProvider serviceProvider, HttpRequestMessage _) => HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(5, (retryAttempt) => TimeSpan.FromMilliseconds(retryAttempt * 300), onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var sourceName = GetSourceName();
                        var logger = serviceProvider.GetRequiredService<ILogger<BlizzardService>>();
                        var correlationId = serviceProvider.GetRequiredService<ICorrelationIdService>().CorrelationId;

                        if (outcome.Exception is TimeoutRejectedException)
                        {
                            logger.LogWarning(sourceName, correlationId, $"Request timed out. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                        }
                        else if (outcome.Result != null)
                        {

                            logger.LogWarning(sourceName, correlationId, $"Request to {outcome.Result.RequestMessage?.RequestUri} failed with status {outcome.Result.StatusCode}. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                        }
                        else
                        {
                            logger.LogWarning(sourceName, correlationId, $"Request failed. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                        }
                    })
                )
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(15));

            services.AddScoped<IBlizzardService, BlizzardService>();

            return services;
        }
    }
}
