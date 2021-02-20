using System;
using System.Threading.Tasks;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.Common.Extensions;
using WoWMarketWatcher.API.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using System.Collections.Generic;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullAuctionDataBackgroundJob
    {
        private readonly BlizzardService blizzardService;
        private readonly WoWItemRepository itemRepository;
        private readonly WatchListRepository watchListRepository;
        private readonly AuctionTimeSeriesRepository timeSeriesRepository;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        public PullAuctionDataBackgroundJob(
            BlizzardService blizzardService,
            WoWItemRepository itemRepository,
            WatchListRepository watchListRepository,
            AuctionTimeSeriesRepository timeSeriesRepository,
            ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService;
            this.itemRepository = itemRepository;
            this.watchListRepository = watchListRepository;
            this.timeSeriesRepository = timeSeriesRepository;
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
                var realmsToUpdate = await this.watchListRepository.EntitySetAsNoTracking().Select(list => list.ConnectedRealmId).Distinct().ToListAsync();
                var currentItems = (await this.itemRepository.EntitySetAsNoTracking().Select(i => i.Id).ToListAsync()).ToHashSet();
                var newItemIds = new HashSet<int>();
                var newAuctionTimeSeriesEntries = new List<AuctionTimeSeriesEntry>();

                context.LogDebug($"{sourceName} ({correlationId}). Fetched {realmsToUpdate.Count} connected realms to update auction data from.");
                this.logger.LogDebug(sourceName, correlationId, $"Fetched {realmsToUpdate.Count} connected realms to update auction data from.");

                foreach (var realmId in realmsToUpdate)
                {
                    context.LogInformation($"{sourceName} ({correlationId}). Processing auction data for connected realm {realmId}.");
                    this.logger.LogInformation(sourceName, correlationId, $"Processing auction data for connected realm {realmId}.");

                    var itemsToUpdate = (await this.watchListRepository.EntitySetAsNoTracking()
                        .Where(list => list.ConnectedRealmId == realmId)
                        .SelectMany(list => list.WatchedItems
                        .Select(item => item.Id))
                        .Distinct()
                        .ToListAsync())
                        .ToHashSet();

                    context.LogDebug($"{sourceName} ({correlationId}). Determined auction data for {itemsToUpdate.Count} items need to be processed based on watch lists for connected realm {realmId}.");
                    this.logger.LogDebug(sourceName, correlationId, $"Determined auction data for {itemsToUpdate.Count} items need to be processed based on watch lists for connected realm {realmId}.");

                    var auctionData = await this.blizzardService.GetAuctionsAsync(realmId);

                    var mappedAuctions = MapAuctionData(auctionData.Auctions, realmId);

                    var newAuctionsToAdd = mappedAuctions.Where(entry => itemsToUpdate.Contains(entry.WoWItemId));

                    newAuctionTimeSeriesEntries.AddRange(newAuctionsToAdd);

                    var newItemIdsFromRealm = mappedAuctions.Select(auc => auc.WoWItemId).Where(id => !currentItems.Contains(id)).ToHashSet();

                    context.LogDebug($"{sourceName} ({correlationId}). Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {realmId}.");
                    this.logger.LogDebug(sourceName, correlationId, $"Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {realmId}.");

                    newItemIds.UnionWith(newItemIdsFromRealm);

                    context.LogInformation($"{sourceName} ({correlationId}). Processing auction data for connected realm {realmId} complete.");
                    this.logger.LogInformation(sourceName, correlationId, $"Processing auction data for connected realm {realmId} complete.");
                }

                try
                {
                    context.LogInformation($"{sourceName} ({correlationId}). Starting to obtain and save data for {newItemIds.Count} newly discovered items.");
                    this.logger.LogInformation(sourceName, correlationId, $"Starting to obtain and save data for {newItemIds.Count} newly discovered items.");

                    var newItemChunks = newItemIds.ChunkBy(100);

                    var newItemChunkedChunks = newItemChunks.ChunkBy(5);

                    var tasks = new List<Task<IEnumerable<WoWItem>>>();

                    context.LogDebug($"{sourceName} ({correlationId}). Processing {newItemChunkedChunks.Count()} chunks of 5 chunks of 100 item ids. Total of {newItemChunks.Count()} chunks of 100 item ids.");
                    this.logger.LogDebug(sourceName, correlationId, $"Processing {newItemChunkedChunks.Count()} chunks of 5 chunks of 100 item ids. Total of {newItemChunks.Count()} chunks of 100 item ids.");

                    foreach (var chunkedChunk in newItemChunkedChunks)
                    {
                        tasks.Add(this.HandleChunkAsync(chunkedChunk));
                    }

                    var itemsFromBlizzard = (await Task.WhenAll(tasks)).SelectMany(item => item);

                    this.itemRepository.AddRange(itemsFromBlizzard);

                    var itemsSaved = await this.itemRepository.SaveChangesAsync();

                    currentItems.UnionWith(itemsFromBlizzard.Select(i => i.Id));

                    context.LogInformation($"{sourceName} ({correlationId}). Obtaining and saving data for {newItemIds.Count} newly discovered items. {itemsSaved} database records created/updated.");
                    this.logger.LogInformation(sourceName, correlationId, $"Obtainint and saving data for {newItemIds.Count} newly discovered items. {itemsSaved} database records created/updated.");
                }
                catch (Exception ex)
                {
                    context.LogWarning($"{sourceName} ({correlationId}). Error while processing data for new items. Auction data will still be processed for all items currently tracked. Reason: {ex.Message}");
                    this.logger.LogWarning(sourceName, correlationId, $"Error while processing data for new items. Auction data will still be processed for all items currently tracked.", ex.Message);
                }

                this.timeSeriesRepository.AddRange(newAuctionTimeSeriesEntries.Where(newAuction => currentItems.Contains(newAuction.WoWItemId)));

                var numberOfEntriesUpdated = await this.timeSeriesRepository.SaveChangesAsync();

                context.LogInformation($"{sourceName} ({correlationId}). {nameof(PullAuctionDataBackgroundJob)} complete. {numberOfEntriesUpdated} auction entries were created/updated.");
                this.logger.LogInformation(sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} complete. {numberOfEntriesUpdated} auction entries were created/updated.");
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

        private static List<AuctionTimeSeriesEntry> MapAuctionData(List<BlizzardAuction> auctions, int connectedRealmId)
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

        private async Task<IEnumerable<WoWItem>> HandleChunkAsync(IEnumerable<IEnumerable<int>> chunkedItemIds)
        {
            var result = new List<WoWItem>();

            foreach (var chunk in chunkedItemIds)
            {
                var res = await this.blizzardService.GetWoWItemsAsync(chunk);
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