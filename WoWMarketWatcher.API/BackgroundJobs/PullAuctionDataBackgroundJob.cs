using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Settings;
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
        private readonly PullAuctionDataBackgroundJobSettings jobSettings;
        private readonly ILogger<PullAuctionDataBackgroundJob> logger;
        private readonly HashSet<int> currentItemIds = new();
        private readonly HashSet<int> newItemIds = new();
        private readonly HashSet<int> itemIdsToAlwaysProcess = new();
        private readonly Dictionary<string, object> logMetadata = new();

        private string hangfireJobId = string.Empty;
        private string correlationId = string.Empty;

        public PullAuctionDataBackgroundJob(
            IBlizzardService blizzardService,
            IWoWItemRepository itemRepository,
            IWatchListRepository watchListRepository,
            IAuctionTimeSeriesRepository timeSeriesRepository,
            IConnectedRealmRepository connectedRealmRepository,
            IOptions<PullAuctionDataBackgroundJobSettings> jobSettings,
            ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.watchListRepository = watchListRepository ?? throw new ArgumentNullException(nameof(watchListRepository));
            this.timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
            this.connectedRealmRepository = connectedRealmRepository ?? throw new ArgumentNullException(nameof(connectedRealmRepository));
            this.jobSettings = jobSettings.Value ?? throw new ArgumentNullException(nameof(jobSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PullAuctionData(PerformContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var sourceName = GetSourceName();
            this.hangfireJobId = context.BackgroundJob.Id;
            this.correlationId = $"{this.hangfireJobId}-{Guid.NewGuid()}";

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullAuctionData));

            this.logMetadata[LogMetadataFields.BackgroundJobName] = nameof(PullAuctionDataBackgroundJob);

            this.logger.LogInformation(this.hangfireJobId, sourceName, this.correlationId, $"{nameof(PullAuctionDataBackgroundJob)} started.", this.logMetadata);

            try
            {
                this.timeSeriesRepository.Context.ChangeTracker.AutoDetectChangesEnabled = false;
                this.timeSeriesRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                this.itemRepository.Context.ChangeTracker.AutoDetectChangesEnabled = false;
                this.itemRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var realmIdsToUpdate = this.jobSettings.AlwaysProcessCertainItemsEnabled
                    ? await this.connectedRealmRepository.EntitySetAsNoTracking().Select(realm => realm.Id).Distinct().ToListAsync()
                    : await this.watchListRepository.EntitySetAsNoTracking().Select(list => list.ConnectedRealmId).Distinct().ToListAsync();
                this.currentItemIds.UnionWith(await this.itemRepository.EntitySetAsNoTracking().Select(i => i.Id).ToListAsync());

                var subclassesToAlwaysProcess = new HashSet<string> { "Cooking", "Herb", "Leather", "Metal & Stone", "Enchanting" };

                if (this.jobSettings.AlwaysProcessCertainItemsEnabled)
                {
                    this.itemIdsToAlwaysProcess.UnionWith(
                        await this.itemRepository.EntitySetAsNoTracking()
                            .Where(item => item.ItemClass == "Tradeskill" && subclassesToAlwaysProcess.Contains(item.ItemSubclass))
                            .Select(item => item.Id).ToListAsync()
                        );
                }

                this.logger.LogInformation(this.hangfireJobId, sourceName, this.correlationId, $"Fetched {realmIdsToUpdate.Count} connected realms to update auction data from with {this.itemIdsToAlwaysProcess.Count} items to always process.", this.logMetadata);

                var numberAuctionEntriesAdded = 0;

                foreach (var connectedRealmId in realmIdsToUpdate)
                {
                    try
                    {
                        var numAuctionEntriesAdded = await this.ProcessConnectedRealmAuctionDataAsync(connectedRealmId);
                        numberAuctionEntriesAdded += numAuctionEntriesAdded;
                        this.itemRepository.Context.ChangeTracker.Clear();
                        this.timeSeriesRepository.Context.ChangeTracker.Clear();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(this.hangfireJobId, sourceName, this.correlationId, $"Failed to process auction data for connected realm {connectedRealmId}. Reason: {ex}", this.logMetadata);
                    }
                }

                var numberNewItemsAdded = await this.ProcessNewItemsAsync();

                stopwatch.Stop();
                this.logMetadata[LogMetadataFields.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.LogInformation(this.hangfireJobId, sourceName, this.correlationId, $"{nameof(PullAuctionDataBackgroundJob)} complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries were created. {numberNewItemsAdded} new items added.", this.logMetadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(this.hangfireJobId, sourceName, this.correlationId, $"{nameof(PullAuctionDataBackgroundJob)} canceled. Reason: {ex}", this.logMetadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(this.hangfireJobId, sourceName, this.correlationId, $"{nameof(PullAuctionDataBackgroundJob)} failed. Reason: {ex}", this.logMetadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
            finally
            {
                this.timeSeriesRepository.Context.ChangeTracker.AutoDetectChangesEnabled = true;
                this.timeSeriesRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
                this.itemRepository.Context.ChangeTracker.AutoDetectChangesEnabled = true;
                this.itemRepository.Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        private async Task<int> ProcessConnectedRealmAuctionDataAsync(int connectedRealmId)
        {
            var sourceName = GetSourceName();

            try
            {
                this.logMetadata[nameof(connectedRealmId)] = connectedRealmId;
                this.logger.LogDebug(this.hangfireJobId, sourceName, this.correlationId, $"Processing auction data for connected realm {connectedRealmId}.", this.logMetadata);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var itemsToUpdate = (await this.watchListRepository.EntitySetAsNoTracking()
                    .Where(list => list.ConnectedRealmId == connectedRealmId)
                    .SelectMany(list => list.WatchedItems
                    .Select(item => item.Id))
                    .Distinct()
                    .ToListAsync())
                    .ToHashSet();

                itemsToUpdate.UnionWith(this.itemIdsToAlwaysProcess);

                this.logger.LogDebug(this.hangfireJobId, sourceName, this.correlationId, $"Determined auction data for {itemsToUpdate.Count} items need to be processed for connected realm {connectedRealmId}.", this.logMetadata);

                var auctionData = await this.blizzardService.GetAuctionsAsync(connectedRealmId, this.correlationId);

                var newItemIdsFromRealm = auctionData.Auctions.Select(auc => auc.Item.Id).Where(id => !this.currentItemIds.Contains(id)).ToHashSet();

                this.logger.LogDebug(this.hangfireJobId, sourceName, this.correlationId, $"Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {connectedRealmId}.", this.logMetadata);

                this.newItemIds.UnionWith(newItemIdsFromRealm);

                var newAuctionsToAdd = this.MapAuctionData(auctionData.Auctions, connectedRealmId, itemsToUpdate);

                this.timeSeriesRepository.AddRange(newAuctionsToAdd);
                var numberAuctionEntriesAdded = await this.timeSeriesRepository.SaveChangesAsync();

                stopwatch.Stop();

                this.logger.LogInformation(this.hangfireJobId, sourceName, this.correlationId, $"Processing auction data for connected realm {connectedRealmId} complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries added and {newItemIdsFromRealm.Count} new items found.", this.logMetadata);

                return numberAuctionEntriesAdded;
            }
            catch (Exception ex)
            {
                this.logger.LogError(this.hangfireJobId, sourceName, this.correlationId, $"Failed to process auction data for connected realm {connectedRealmId}. Reason: {ex}", this.logMetadata);

                return 0;
            }
            finally
            {
                this.logMetadata.Remove(nameof(connectedRealmId));
            }
        }

        private async Task<int> ProcessNewItemsAsync()
        {
            var sourceName = GetSourceName();

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                this.logger.LogDebug(this.hangfireJobId, sourceName, this.correlationId, $"Starting to obtain and save data for {this.newItemIds.Count} newly discovered items.", this.logMetadata);

                var newItemChunks = this.newItemIds.ChunkBy(100);

                var newItemChunkedChunks = newItemChunks.ChunkBy(5);

                var tasks = new List<Task<IEnumerable<WoWItem>>>();

                foreach (var chunkedChunk in newItemChunkedChunks)
                {
                    tasks.Add(this.HandleChunkedItemRequestsAsync(chunkedChunk));
                }

                var itemsFromBlizzard = (await Task.WhenAll(tasks)).SelectMany(item => item);

                this.itemRepository.AddRange(itemsFromBlizzard);

                var itemsSaved = await this.itemRepository.SaveChangesAsync();

                stopwatch.Stop();
                this.logger.LogDebug(this.hangfireJobId, sourceName, this.correlationId, $"Obtaining and saving data for {this.newItemIds.Count} newly discovered items complete in {stopwatch.ElapsedMilliseconds}ms. {itemsSaved} database records created.", this.logMetadata);

                return itemsSaved;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(this.hangfireJobId, sourceName, this.correlationId, $"Error while processing data for new items. Reason: {ex}", this.logMetadata);

                return 0;
            }
        }

        private List<AuctionTimeSeriesEntry> MapAuctionData(List<BlizzardAuction> auctions, int connectedRealmId, HashSet<int> itemIdsToProcess)
        {
            var sourceName = GetSourceName();

            var itemIdAuctionMap = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in auctions)
            {
                if (!itemIdsToProcess.Contains(auction.Item.Id) || !this.currentItemIds.Contains(auction.Item.Id))
                {
                    continue;
                }

                var price = auction.Buyout ?? auction.UnitPrice ?? auction.Bid;

                if (price == null)
                {
                    this.logger.LogWarning(this.hangfireJobId, sourceName, this.correlationId, $"Auction {auction.Id} does not have a buyout or unit price.", this.logMetadata);
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

        private async Task<IEnumerable<WoWItem>> HandleChunkedItemRequestsAsync(IEnumerable<IEnumerable<int>> chunkedItemIds)
        {
            var sourceName = GetSourceName();
            var result = new List<WoWItem>();

            foreach (var chunk in chunkedItemIds)
            {
                try
                {
                    var res = await this.blizzardService.GetWoWItemsAsync(chunk, this.correlationId);
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
                    this.logger.LogError(this.hangfireJobId, sourceName, this.correlationId, $"Failed to handle chunk of {chunk.Count()} items. Reason: {ex}", this.logMetadata);
                }
            }

            return result;
        }
    }
}