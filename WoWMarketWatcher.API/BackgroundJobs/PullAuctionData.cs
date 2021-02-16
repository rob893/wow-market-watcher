using System.Threading.Tasks;
using Google.Apis.Logging;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullAuctionDataBackgroundJob
    {
        private readonly BlizzardService blizzardService;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        public PullAuctionDataBackgroundJob(BlizzardService blizzardService, ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService;
            this.logger = logger;
        }

        public async Task PullAuctionData(PerformContext context)
        {
            context.LogInformation("Hello world from background job LOL!");
            this.logger.LogInformation("Hello world from background job LOL!");
        }
    }
}