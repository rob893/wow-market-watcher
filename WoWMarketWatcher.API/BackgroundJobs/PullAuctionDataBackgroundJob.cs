using System;
using System.Threading.Tasks;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using System.Collections.Generic;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Extensions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullAuctionDataBackgroundJob
    {
        private readonly IBlizzardService blizzardService;
        private readonly IWoWItemRepository itemRepository;
        private readonly IWatchListRepository watchListRepository;
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        public PullAuctionDataBackgroundJob(
            IBlizzardService blizzardService,
            IWoWItemRepository itemRepository,
            IWatchListRepository watchListRepository,
            IAuctionTimeSeriesRepository timeSeriesRepository,
            ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.watchListRepository = watchListRepository ?? throw new ArgumentNullException(nameof(watchListRepository));
            this.timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PullAuctionData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = this.GetSourceName();
            var jobId = context.BackgroundJob.Id;
            var correlationId = $"{jobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullAuctionData));

            this.logger.LogInformation(jobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} started.");

            try
            {
                var realmsToUpdate = await this.watchListRepository.EntitySetAsNoTracking().Select(list => list.ConnectedRealmId).Distinct().ToListAsync();
                var currentItems = (await this.itemRepository.EntitySetAsNoTracking().Select(i => i.Id).ToListAsync()).ToHashSet();
                var newItemIds = new HashSet<int>();
                var newAuctionTimeSeriesEntries = new List<AuctionTimeSeriesEntry>();

                this.logger.LogDebug(jobId, sourceName, correlationId, $"Fetched {realmsToUpdate.Count} connected realms to update auction data from.");

                foreach (var realmId in realmsToUpdate)
                {
                    this.logger.LogInformation(jobId, sourceName, correlationId, $"Processing auction data for connected realm {realmId}.");

                    var itemsToUpdate = (await this.watchListRepository.EntitySetAsNoTracking()
                        .Where(list => list.ConnectedRealmId == realmId)
                        .SelectMany(list => list.WatchedItems
                        .Select(item => item.Id))
                        .Distinct()
                        .ToListAsync())
                        .ToHashSet();

                    this.logger.LogDebug(jobId, sourceName, correlationId, $"Determined auction data for {itemsToUpdate.Count} items need to be processed based on watch lists for connected realm {realmId}.");

                    var auctionData = await this.blizzardService.GetAuctionsAsync(realmId, correlationId);

                    var newAuctionsToAdd = MapAuctionData(auctionData.Auctions, realmId, itemsToUpdate);

                    newAuctionTimeSeriesEntries.AddRange(newAuctionsToAdd);

                    var newItemIdsFromRealm = auctionData.Auctions.Select(auc => auc.Item.Id).Where(id => !currentItems.Contains(id)).ToHashSet();

                    this.logger.LogDebug(jobId, sourceName, correlationId, $"Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {realmId}.");

                    newItemIds.UnionWith(newItemIdsFromRealm);

                    this.logger.LogInformation(jobId, sourceName, correlationId, $"Processing auction data for connected realm {realmId} complete.");
                }

                try
                {
                    this.logger.LogInformation(jobId, sourceName, correlationId, $"Starting to obtain and save data for {newItemIds.Count} newly discovered items.");

                    var newItemChunks = newItemIds.ChunkBy(100);

                    var newItemChunkedChunks = newItemChunks.ChunkBy(5);

                    var tasks = new List<Task<IEnumerable<WoWItem>>>();

                    this.logger.LogDebug(jobId, sourceName, correlationId, $"Processing {newItemChunkedChunks.Count()} chunks of 5 chunks of 100 item ids. Total of {newItemChunks.Count()} chunks of 100 item ids.");

                    foreach (var chunkedChunk in newItemChunkedChunks)
                    {
                        tasks.Add(this.HandleChunkAsync(chunkedChunk, correlationId));
                    }

                    var itemsFromBlizzard = (await Task.WhenAll(tasks)).SelectMany(item => item);

                    this.itemRepository.AddRange(itemsFromBlizzard);

                    var itemsSaved = await this.itemRepository.SaveChangesAsync();

                    currentItems.UnionWith(itemsFromBlizzard.Select(i => i.Id));

                    this.logger.LogInformation(jobId, sourceName, correlationId, $"Obtaining and saving data for {newItemIds.Count} newly discovered items. {itemsSaved} database records created/updated.");
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(jobId, sourceName, correlationId, $"Error while processing data for new items. Auction data will still be processed for all items currently tracked. Reason: {ex.Message}", ex);
                }

                this.timeSeriesRepository.AddRange(newAuctionTimeSeriesEntries.Where(newAuction => currentItems.Contains(newAuction.WoWItemId)));

                var numberOfEntriesUpdated = await this.timeSeriesRepository.SaveChangesAsync();

                this.logger.LogInformation(jobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} complete. {numberOfEntriesUpdated} auction entries were created/updated.");
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(jobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} canceled. Reason: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                this.logger.LogError(jobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} failed. Reason: {ex.Message}", ex);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
        }

        private static List<AuctionTimeSeriesEntry> MapAuctionData(List<BlizzardAuction> auctions, int connectedRealmId, HashSet<int> itemIdsToProcess)
        {
            var itemIdAuctionMap = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in auctions)
            {
                if (!itemIdsToProcess.Contains(auction.Item.Id))
                {
                    continue;
                }

                var price = auction.Buyout ?? auction.UnitPrice ?? auction.Bid;

                if (price == null)
                {
                    throw new Exception($"auction {auction.Id} does not have a buyout or unit price");
                }

                if (itemIdAuctionMap.ContainsKey(auction.Item.Id))
                {
                    var prices = seen[auction.Item.Id];
                    prices.Add((amount: auction.Quantity, price: price.Value));

                    var timeSeries = itemIdAuctionMap[auction.Item.Id];

                    timeSeries.TotalAvailableForAuction += auction.Quantity;
                    timeSeries.MinPrice = timeSeries.MinPrice > price.Value ? price.Value : timeSeries.MinPrice;
                    timeSeries.MaxPrice = timeSeries.MaxPrice < price.Value ? price.Value : timeSeries.MaxPrice;
                }
                else
                {
                    itemIdAuctionMap[auction.Item.Id] = new AuctionTimeSeriesEntry
                    {
                        WoWItemId = auction.Item.Id,
                        ConnectedRealmId = connectedRealmId,
                        Timestamp = utcNow,
                        TotalAvailableForAuction = auction.Quantity,
                        AveragePrice = price.Value,
                        MinPrice = price.Value,
                        MaxPrice = price.Value
                    };

                    seen[auction.Item.Id] = new List<(long amount, long price)> { (amount: auction.Quantity, price: price.Value) };
                }
            }

            foreach (var value in itemIdAuctionMap.Values)
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

                value.Price25Percentile = sortedPrices.Percentile(0.25, true);
                value.Price50Percentile = sortedPrices.Percentile(0.50, true);
                value.Price75Percentile = sortedPrices.Percentile(0.75, true);
                value.Price95Percentile = sortedPrices.Percentile(0.95, true);
                value.Price99Percentile = sortedPrices.Percentile(0.99, true);
            }

            return itemIdAuctionMap.Values.ToList();
        }

        private async Task<IEnumerable<WoWItem>> HandleChunkAsync(IEnumerable<IEnumerable<int>> chunkedItemIds, string correlationId)
        {
            var result = new List<WoWItem>();

            foreach (var chunk in chunkedItemIds)
            {
                var res = await this.blizzardService.GetWoWItemsAsync(chunk, correlationId);
                result.AddRange(res.Results.Select(r => new WoWItem
                {
                    Id = r.Data.Id,
                    Name = r.Data.Name.EnUS,
                    IsEquippable = r.Data.IsEquippable,
                    IsStackable = r.Data.IsStackable,
                    Level = r.Data.Level,
                    RequiredLevel = r.Data.RequiredLevel,
                    SellPrice = r.Data.SellPrice,
                    PurchaseQuantity = r.Data.PurchaseQuantity,
                    PurchasePrice = r.Data.PurchasePrice,
                    ItemClass = r.Data.ItemClass.Name.EnUS,
                    ItemSubclass = r.Data.ItemSubclass.Name.EnUS,
                    Quality = r.Data.Quality.Name.EnUS,
                    InventoryType = r.Data.InventoryType.Name.EnUS,
                    MaxCount = r.Data.MaxCount
                }));
            }

            return result;
        }
    }
}