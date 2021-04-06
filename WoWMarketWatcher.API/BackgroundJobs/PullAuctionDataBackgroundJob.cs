using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Services;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public class PullAuctionDataBackgroundJob
    {
        private readonly IBlizzardService blizzardService;
        private readonly IWoWItemRepository itemRepository;
        private readonly IWatchListRepository watchListRepository;
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;
        private readonly IConnectedRealmRepository connectedRealmRepository;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;
        private readonly ConcurrentHashSet<int> currentItemIds = new();

        public PullAuctionDataBackgroundJob(
            IBlizzardService blizzardService,
            IWoWItemRepository itemRepository,
            IWatchListRepository watchListRepository,
            IAuctionTimeSeriesRepository timeSeriesRepository,
            IConnectedRealmRepository connectedRealmRepository,
            ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.watchListRepository = watchListRepository ?? throw new ArgumentNullException(nameof(watchListRepository));
            this.timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
            this.connectedRealmRepository = connectedRealmRepository ?? throw new ArgumentNullException(nameof(connectedRealmRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PullAuctionData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = GetSourceName();
            var hangfireJobId = context.BackgroundJob.Id;
            var correlationId = $"{hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullAuctionData));

            var logMetadata = new Dictionary<string, object> { { LogMetadataFields.BackgroundJobName, nameof(PullAuctionDataBackgroundJob) } };

            this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} started.", logMetadata);

            var processAllRealms = true;

            try
            {
                this.timeSeriesRepository.Context.ChangeTracker.AutoDetectChangesEnabled = false;
                this.timeSeriesRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var realmIdsToUpdate = processAllRealms
                    ? await this.connectedRealmRepository.EntitySetAsNoTracking().Select(realm => realm.Id).Distinct().ToListAsync()
                    : await this.watchListRepository.EntitySetAsNoTracking().Select(list => list.ConnectedRealmId).Distinct().ToListAsync();
                this.currentItemIds.UnionWith(await this.itemRepository.EntitySetAsNoTracking().Select(i => i.Id).ToListAsync());

                this.logger.LogDebug(hangfireJobId, sourceName, correlationId, $"Fetched {realmIdsToUpdate.Count} connected realms to update auction data from.", logMetadata);

                var chunkedRealms = realmIdsToUpdate.ChunkBy(400);
                var tasks = new List<Task<(int numberNewItemsAdded, int numberAuctionEntriesAdded)>>();

                foreach (var chunk in chunkedRealms)
                {
                    tasks.Add(this.HandleConnectedRealmChunkAsync(chunk, correlationId, hangfireJobId, logMetadata));
                }

                var (numberNewItemsAdded, numberAuctionEntriesAdded) = (await Task.WhenAll(tasks))
                    .Aggregate(
                        (prev, curr) =>
                            (numberNewItemsAdded: prev.numberNewItemsAdded + curr.numberNewItemsAdded, numberAuctionEntriesAdded: prev.numberAuctionEntriesAdded + curr.numberAuctionEntriesAdded)
                    );

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} complete. {numberAuctionEntriesAdded} auction entries were created. {numberNewItemsAdded} new items added.", logMetadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} canceled. Reason: {ex}", logMetadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, correlationId, $"{nameof(PullAuctionDataBackgroundJob)} failed. Reason: {ex}", logMetadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
            finally
            {
                this.timeSeriesRepository.Context.ChangeTracker.AutoDetectChangesEnabled = true;
                this.timeSeriesRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        private async Task<(int numberNewItemsAdded, int numberAuctionEntriesAdded)> HandleConnectedRealmChunkAsync(
            IEnumerable<int> connectedRealmIdChunk,
            string correlationId,
            string hangfireJobId,
            Dictionary<string, object> logMetadata)
        {
            var sourceName = GetSourceName();
            var realmsStr = connectedRealmIdChunk.Aggregate("", (prev, curr) => $"{prev}{curr}, ");

            this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Processing auction data for {connectedRealmIdChunk.Count()} realms: {realmsStr}");

            var numberNewItemsAdded = 0;
            var numberAuctionEntriesAdded = 0;

            foreach (var connectedRealmId in connectedRealmIdChunk)
            {
                try
                {
                    var (numNewItemsAdded, numAuctionEntriesAdded) = await this.ProcessConnectedRealmAuctionDataAsync(connectedRealmId, true, correlationId, hangfireJobId, logMetadata);
                    numberNewItemsAdded += numNewItemsAdded;
                    numberAuctionEntriesAdded += numAuctionEntriesAdded;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(hangfireJobId, sourceName, correlationId, $"Failed to process auction data for connected realm {connectedRealmId}. Reason: {ex}", logMetadata);
                }
            }

            this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Processing auction data for {connectedRealmIdChunk.Count()} realms: {realmsStr} complete.");

            return (numberNewItemsAdded, numberAuctionEntriesAdded);
        }

        private async Task<(int numberNewItemsAdded, int numberAuctionEntriesAdded)> ProcessConnectedRealmAuctionDataAsync(
            int connectedRealmId,
            bool processAllItems,
            string correlationId,
            string hangfireJobId,
            Dictionary<string, object> logMetadata)
        {
            var sourceName = GetSourceName();

            var numberNewItemsAdded = 0;
            var numberAuctionEntriesAdded = 0;

            try
            {
                logMetadata[nameof(connectedRealmId)] = connectedRealmId;
                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Processing auction data for connected realm {connectedRealmId}.", logMetadata);

                var itemsToUpdate = processAllItems ? this.currentItemIds.ToHashSet() : (await this.watchListRepository.EntitySetAsNoTracking()
                    .Where(list => list.ConnectedRealmId == connectedRealmId)
                    .SelectMany(list => list.WatchedItems
                    .Select(item => item.Id))
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet();

                this.logger.LogDebug(hangfireJobId, sourceName, correlationId, $"Determined auction data for {itemsToUpdate.Count} items need to be processed based on watch lists for connected realm {connectedRealmId}.", logMetadata);

                var auctionData = await this.blizzardService.GetAuctionsAsync(connectedRealmId, correlationId);

                var newItemIdsFromRealm = auctionData.Auctions.Select(auc => auc.Item.Id).Where(id => !this.currentItemIds.Contains(id)).ToHashSet();

                this.logger.LogDebug(hangfireJobId, sourceName, correlationId, $"Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {connectedRealmId}.", logMetadata);

                numberNewItemsAdded += await this.ProcessNewItemsAsync(newItemIdsFromRealm, correlationId, hangfireJobId, logMetadata);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var newAuctionsToAdd = this.MapAuctionData(auctionData.Auctions, connectedRealmId, itemsToUpdate, processAllItems, correlationId, hangfireJobId, logMetadata);
                stopwatch.Stop();

                this.logger.LogDebug(hangfireJobId, sourceName, correlationId, $"Mapping complete in {stopwatch.ElapsedMilliseconds}ms. Saving...");

                stopwatch.Reset();
                stopwatch.Start();


                // var chunkedAuctions = newAuctionsToAdd.ChunkBy(10000);

                // foreach (var chunk in chunkedAuctions)
                // {
                this.timeSeriesRepository.AddRange(newAuctionsToAdd);
                numberAuctionEntriesAdded += await this.timeSeriesRepository.SaveChangesAsync();
                // }

                stopwatch.Stop();

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Processing auction data for connected realm {connectedRealmId} complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries added.", logMetadata);

                return (numberNewItemsAdded, numberAuctionEntriesAdded);
            }
            catch (Exception ex)
            {
                this.logger.LogError(hangfireJobId, sourceName, correlationId, $"Failed to process auction data for connected realm {connectedRealmId}. Reason: {ex}", logMetadata);

                return (numberNewItemsAdded, numberAuctionEntriesAdded);
            }
            finally
            {
                logMetadata.Remove(nameof(connectedRealmId));
            }
        }

        private async Task<int> ProcessNewItemsAsync(HashSet<int> newItemIds,
            string correlationId,
            string hangfireJobId,
            Dictionary<string, object> logMetadata)
        {
            var sourceName = GetSourceName();

            try
            {
                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Starting to obtain and save data for {newItemIds.Count} newly discovered items.", logMetadata);

                var newItemChunks = newItemIds.ChunkBy(100);

                var newItemChunkedChunks = newItemChunks.ChunkBy(5);

                var tasks = new List<Task<IEnumerable<WoWItem>>>();

                this.logger.LogDebug(hangfireJobId, sourceName, correlationId, $"Processing {newItemChunkedChunks.Count()} chunks of 5 chunks of 100 item ids. Total of {newItemChunks.Count()} chunks of 100 item ids.", logMetadata);

                foreach (var chunkedChunk in newItemChunkedChunks)
                {
                    tasks.Add(this.HandleChunkedItemRequestsAsync(chunkedChunk, correlationId, hangfireJobId, logMetadata));
                }

                var itemsFromBlizzard = (await Task.WhenAll(tasks)).SelectMany(item => item);

                this.itemRepository.AddRange(itemsFromBlizzard);

                var itemsSaved = await this.itemRepository.SaveChangesAsync();

                this.currentItemIds.UnionWith(itemsFromBlizzard.Select(i => i.Id));

                this.logger.LogInformation(hangfireJobId, sourceName, correlationId, $"Obtaining and saving data for {newItemIds.Count} newly discovered items. {itemsSaved} database records created/updated.", logMetadata);

                return itemsSaved;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(hangfireJobId, sourceName, correlationId, $"Error while processing data for new items. Auction data will still be processed for all items currently tracked. Reason: {ex}", logMetadata);

                return 0;
            }
        }

        private List<AuctionTimeSeriesEntry> MapAuctionData(
            List<BlizzardAuction> auctions,
            int connectedRealmId,
            HashSet<int> itemIdsToProcess,
            bool processAllItems,
            string correlationId,
            string hangfireJobId,
            Dictionary<string, object> logMetadata)
        {
            var sourceName = GetSourceName();

            var itemIdAuctionMap = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in auctions)
            {
                if (!this.currentItemIds.Contains(auction.Item.Id) || (!processAllItems && !itemIdsToProcess.Contains(auction.Item.Id)))
                {
                    continue;
                }

                var price = auction.Buyout ?? auction.UnitPrice ?? auction.Bid;

                if (price == null)
                {
                    this.logger.LogWarning(hangfireJobId, sourceName, correlationId, $"Auction {auction.Id} does not have a buyout or unit price.", logMetadata);
                    continue;
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

        private async Task<IEnumerable<WoWItem>> HandleChunkedItemRequestsAsync(
            IEnumerable<IEnumerable<int>> chunkedItemIds,
            string correlationId,
            string hangfireJobId,
            Dictionary<string, object> logMetadata)
        {
            var sourceName = GetSourceName();
            var result = new List<WoWItem>();

            foreach (var chunk in chunkedItemIds)
            {
                try
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
                catch (Exception ex)
                {
                    this.logger.LogError(hangfireJobId, sourceName, correlationId, $"Failed to handle chunk of {chunk.Count()} items. Reason: {ex}", logMetadata);
                }
            }

            return result;
        }
    }
}