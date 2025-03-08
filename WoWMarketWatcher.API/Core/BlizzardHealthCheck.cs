using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Core
{
    public sealed class BlizzardHealthCheck : IHealthCheck
    {
        private static readonly TimeSpan cacheDuration = TimeSpan.FromSeconds(15);

        private static readonly object cacheLock = new();

        private static DateTime? lastHealthyCheck;

        private readonly IBlizzardService blizzardService;

        public BlizzardHealthCheck(IBlizzardService blizzardService)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var details = new Dictionary<string, object>
            {
                { "cached", false },
                { nameof(lastHealthyCheck), lastHealthyCheck }
            };

            try
            {
                if (lastHealthyCheck != null && lastHealthyCheck.Value.Add(cacheDuration) > DateTime.UtcNow)
                {
                    details["cached"] = true;
                    return HealthCheckResult.Healthy("Response received from Blizzard API.", details);
                }

                var response = await this.blizzardService.GetWoWTokenPriceAsync(cancellationToken).ConfigureAwait(false);

                if (response == null)
                {
                    return HealthCheckResult.Unhealthy("No response from Blizzard API.", data: details);
                }

                lock (cacheLock)
                {
                    lastHealthyCheck = DateTime.UtcNow;
                }

                details[nameof(lastHealthyCheck)] = lastHealthyCheck;

                var healthyResult = HealthCheckResult.Healthy("Response received from Blizzard API.", details);

                return healthyResult;
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message, e, details);
            }
        }
    }
}