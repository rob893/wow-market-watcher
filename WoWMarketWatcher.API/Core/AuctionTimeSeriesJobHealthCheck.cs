using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WoWMarketWatcher.API.Data.Repositories;

namespace WoWMarketWatcher.API.Core
{
    public class AuctionTimeSeriesJobHealthCheck : IHealthCheck
    {
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;

        public AuctionTimeSeriesJobHealthCheck(IAuctionTimeSeriesRepository timeSeriesRepository)
        {
            this.timeSeriesRepository = timeSeriesRepository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await this.timeSeriesRepository.Context.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy("Cannot connect to database.");
                }

                var entry = await this.timeSeriesRepository.EntitySetAsNoTracking()
                    .Where(entry => entry.Timestamp > DateTime.UtcNow.AddHours(-12))
                    .FirstOrDefaultAsync(cancellationToken);

                if (entry == null)
                {
                    return HealthCheckResult.Unhealthy("No auction time series added in past 12 hours.");
                }

                return HealthCheckResult.Healthy("Auction time series have been added in past 12 hours.");
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(e.Message);
            }
        }
    }
}