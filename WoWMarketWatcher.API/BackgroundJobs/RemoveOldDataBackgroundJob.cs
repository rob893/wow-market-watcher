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
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class RemoveOldDataBackgroundJob
    {
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        public RemoveOldDataBackgroundJob(IAuctionTimeSeriesRepository timeSeriesRepository, ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RemoveOldData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = GetSourceName();
            var hangfireJobId = context.BackgroundJob.Id;
            var correlationId = $"{hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(RemoveOldData));

            var metadata = new Dictionary<string, object> { { LogMetadataFields.BackgroundJobName, nameof(RemoveOldDataBackgroundJob) } };

            this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(RemoveOldDataBackgroundJob)} started.", metadata);

            try
            {
                var deleteBefore = DateTime.UtcNow.AddDays(-30);

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Searching for all entries that were created before {deleteBefore}.", metadata);

                var limit = 10000;
                var hasMoreToDelete = false;
                var totalDeleted = 0;

                do
                {
                    var entriesToDelete = await this.timeSeriesRepository
                        .EntitySetAsNoTracking()
                        .Where(entry => entry.Timestamp < deleteBefore)
                        .OrderBy(entry => entry.Id)
                        .Take(limit + 1)
                        .ToListAsync();

                    if (entriesToDelete.Count == 0)
                    {
                        this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(RemoveOldDataBackgroundJob)} complete. {totalDeleted} old auction entries were successfully deleted.", metadata);
                        return;
                    }

                    if (entriesToDelete.Count > limit)
                    {
                        hasMoreToDelete = true;
                        entriesToDelete.RemoveAt(entriesToDelete.Count - 1);
                    }

                    this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Deleting {entriesToDelete.Count} entries.", metadata);

                    this.timeSeriesRepository.DeleteRange(entriesToDelete);

                    var deleted = await this.timeSeriesRepository.SaveChangesAsync();
                    totalDeleted += deleted;

                    this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{deleted} entries deleted. More to delete: {hasMoreToDelete}", metadata);
                }
                while (hasMoreToDelete);

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(RemoveOldDataBackgroundJob)} complete. {totalDeleted} old auction entries were successfully deleted.", metadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, correlationId, $"{nameof(RemoveOldDataBackgroundJob)} canceled. Reason: {ex}", metadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, correlationId, $"{nameof(RemoveOldDataBackgroundJob)} failed. Reason: {ex}", metadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }
    }
}