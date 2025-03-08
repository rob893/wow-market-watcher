using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WoWMarketWatcher.API.Data;

namespace WoWMarketWatcher.API.Core
{
    public sealed class AuctionTimeSeriesJobHealthCheck : IHealthCheck
    {
        private readonly DataContext dbContext;

        public AuctionTimeSeriesJobHealthCheck(DataContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await this.dbContext.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy("Cannot connect to database.");
                }

                var entry = await this.dbContext.AuctionTimeSeries.AsNoTracking()
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
                return HealthCheckResult.Unhealthy(e.Message, e);
            }
        }
    }
}