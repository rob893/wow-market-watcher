using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Services;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public sealed class RemoveOldDataBackgroundJob
    {
        private readonly DataContext dbContext;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        public RemoveOldDataBackgroundJob(DataContext dbContext, ICorrelationIdService correlationIdService, ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public async Task RemoveOldData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = GetSourceName();
            var hangfireJobId = context.BackgroundJob.Id;
            this.correlationIdService.CorrelationId = $"{hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(RemoveOldData));

            var metadata = new Dictionary<string, object> { { LogMetadataFields.BackgroundJobName, nameof(RemoveOldDataBackgroundJob) } };

            this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(RemoveOldDataBackgroundJob)} started.", metadata);

            try
            {
                var deleteBefore = DateTime.UtcNow.AddDays(-30);

                this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"Searching for all entries that were created before {deleteBefore}.", metadata);

                var limit = 10000;
                var hasMoreToDelete = false;
                var totalDeleted = 0;

                do
                {
                    var entriesToDelete = await this.dbContext.AuctionTimeSeries
                        .AsNoTracking()
                        .Where(entry => entry.Timestamp < deleteBefore)
                        .OrderBy(entry => entry.Id)
                        .Take(limit + 1)
                        .ToListAsync();

                    if (entriesToDelete.Count == 0)
                    {
                        this.logger.LogInformation(
                            hangfireJobId,
                            sourceName,
                            this.CorrelationId,
                            $"{nameof(RemoveOldDataBackgroundJob)} complete. {totalDeleted} old auction entries were successfully deleted.",
                            metadata);
                        return;
                    }

                    if (entriesToDelete.Count > limit)
                    {
                        hasMoreToDelete = true;
                        entriesToDelete.RemoveAt(entriesToDelete.Count - 1);
                    }

                    this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"Deleting {entriesToDelete.Count} entries.", metadata);

                    this.dbContext.AuctionTimeSeries.RemoveRange(entriesToDelete);

                    var deleted = await this.dbContext.SaveChangesAsync();
                    totalDeleted += deleted;

                    this.logger.LogInformation(hangfireJobId, sourceName, this.CorrelationId, $"{deleted} entries deleted. More to delete: {hasMoreToDelete}", metadata);
                }
                while (hasMoreToDelete);

                this.logger.LogInformation(
                    hangfireJobId,
                    sourceName,
                    this.CorrelationId,
                    $"{nameof(RemoveOldDataBackgroundJob)} complete. {totalDeleted} old auction entries were successfully deleted.",
                    metadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(RemoveOldDataBackgroundJob)} canceled. Reason: {ex}", metadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, this.CorrelationId, $"{nameof(RemoveOldDataBackgroundJob)} failed. Reason: {ex}", metadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }
    }
}