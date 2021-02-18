using System;
using System.Threading.Tasks;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.Common.Extensions;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.API.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using System.Collections.Generic;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.Responses.Blizzard;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullAuctionDataBackgroundJob
    {
        private readonly BlizzardService blizzardService;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;
        private readonly DataContext dbContext;

        public PullAuctionDataBackgroundJob(BlizzardService blizzardService, DataContext dbContext, ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task PullAuctionData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = this.GetSourceName();
            var correlationId = context.BackgroundJob.Id;

            context.LogInformation($"{sourceName} ({correlationId}). {nameof(PullAuctionDataBackgroundJob)} started.");
            this.logger.LogInformation(sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} started.");

            try
            {
                var realmsToUpdate = await this.dbContext.WatchLists.Select(list => list.ConnectedRealmId).Distinct().ToListAsync();
                var currentItems = (await this.dbContext.WoWItems.Select(i => i.Id).ToListAsync()).ToHashSet();

                context.LogDebug($"{sourceName} ({correlationId}). Fetched {realmsToUpdate.Count} connected realms to update auction data from.");
                this.logger.LogDebug(sourceName, correlationId, $"Fetched {realmsToUpdate.Count} connected realms to update auction data from.");

                foreach (var realmId in realmsToUpdate)
                {
                    context.LogInformation($"{sourceName} ({correlationId}). Updating auction data for connected realm {realmId}.");
                    this.logger.LogInformation(sourceName, correlationId, $"Updating auction data for connected realm {realmId}.");

                    var itemsToUpdate = (await this.dbContext.WatchLists
                        .Where(list => list.ConnectedRealmId == realmId)
                        .SelectMany(list => list.WatchedItems
                        .Select(item => item.Id))
                        .Distinct()
                        .ToListAsync())
                        .ToHashSet();

                    var auctionData = await this.blizzardService.GetAuctionsAsync(realmId);

                    var mapped = MapAuctionData(auctionData.Auctions);

                    var dataToAdd = mapped.Where(entry => itemsToUpdate.Contains(entry.WoWItemId) && currentItems.Contains(entry.WoWItemId));

                    this.dbContext.AuctionTimeSeries.AddRange(dataToAdd);

                    context.LogInformation($"{sourceName} ({correlationId}). Updating auction data for connected realm {realmId} complete.");
                    this.logger.LogInformation(sourceName, correlationId, $"Updating auction data for connected realm {realmId} complete.");
                }

                await this.dbContext.SaveChangesAsync();

                context.LogInformation($"{sourceName} ({correlationId}). {nameof(PullAuctionDataBackgroundJob)} complete.");
                this.logger.LogInformation(sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} complete.");
            }
            catch (OperationCanceledException ex)
            {
                context.LogWarning($"{sourceName} ({correlationId}). {nameof(PullAuctionDataBackgroundJob)} canceled. Reason: {ex.Message}");
                this.logger.LogWarning(sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} canceled.", ex.Message);
            }
            catch (Exception ex)
            {
                context.LogError($"{sourceName} ({correlationId}). {nameof(PullAuctionDataBackgroundJob)} failed. Reason: {ex.Message}");
                this.logger.LogError(sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} failed.", ex);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }

        private static List<AuctionTimeSeriesEntry> MapAuctionData(List<BlizzardAuction> auctions)
        {
            var dict = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in auctions)
            {
                var price = auction.Buyout ?? auction.UnitPrice ?? auction.Bid;

                if (price == null)
                {
                    throw new Exception($"auction {auction.Id} does not have a buyout or unit price");
                }

                if (dict.ContainsKey(auction.Item.Id))
                {
                    var prices = seen[auction.Item.Id];
                    prices.Add((amount: auction.Quantity, price: price.Value));

                    var timeSeries = dict[auction.Item.Id];

                    timeSeries.TotalAvailableForAuction += auction.Quantity;
                    timeSeries.MinPrice = timeSeries.MinPrice > price.Value ? price.Value : timeSeries.MinPrice;
                    timeSeries.MaxPrice = timeSeries.MaxPrice < price.Value ? price.Value : timeSeries.MaxPrice;
                }
                else
                {
                    dict[auction.Item.Id] = new AuctionTimeSeriesEntry
                    {
                        WoWItemId = auction.Item.Id,
                        ConnectedRealmId = 3694,
                        Timestamp = utcNow,
                        TotalAvailableForAuction = auction.Quantity,
                        AveragePrice = price.Value,
                        MinPrice = price.Value,
                        MaxPrice = price.Value
                    };

                    seen[auction.Item.Id] = new List<(long amount, long price)> { (amount: auction.Quantity, price: price.Value) };
                }
            }

            foreach (var value in dict.Values)
            {
                var seenPrices = seen[value.WoWItemId];
                var prices = seenPrices.Aggregate(new List<long>(), (prev, curr) =>
                {
                    prev.AddRange(Enumerable.Repeat(curr.price, (int)curr.amount));
                    return prev;
                });

                value.AveragePrice = seenPrices.Aggregate(0L, (prev, curr) => prev + (curr.amount * curr.price)) / seenPrices.Aggregate(0L, (prev, curr) => prev + curr.amount);

                var sortedPrices = prices.ToArray();
                Array.Sort(sortedPrices);

                value.Price25Percentile = Percentile(sortedPrices, 0.25, true);
                value.Price50Percentile = Percentile(sortedPrices, 0.50, true);
                value.Price75Percentile = Percentile(sortedPrices, 0.75, true);
                value.Price95Percentile = Percentile(sortedPrices, 0.95, true);
                value.Price99Percentile = Percentile(sortedPrices, 0.99, true);
            }

            return dict.Values.ToList();
        }

        private static long Percentile(long[] source, double percentile, bool isSourceSorted = false)
        {
            if (percentile < 0 || percentile > 1)
            {
                throw new ArgumentException($"{nameof(percentile)} must be >= 0 and <= 1");
            }

            var index = (int)Math.Floor(percentile * (source.Length - 1));

            if (isSourceSorted)
            {
                return source[index];
            }

            var copy = source.ToArray();
            Array.Sort(copy);

            return copy[index];
        }
    }
}