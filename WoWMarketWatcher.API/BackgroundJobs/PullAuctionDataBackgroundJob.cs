using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.JobsLogger;
using Hangfire.Server;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.API.Services.Events;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.BackgroundJobs
{
    public sealed class PullAuctionDataBackgroundJob
    {
        private readonly DataContext dbContext;

        private readonly IBlizzardService blizzardService;

        private readonly IEventGridEventSender eventGridEventSender;

        private readonly PullAuctionDataBackgroundJobSettings jobSettings;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<PullAuctionDataBackgroundJob> logger;

        private readonly HashSet<int> currentItemIds = new();

        private readonly HashSet<int> newItemIds = new();

        private readonly HashSet<int> itemIdsToAlwaysProcess = new();

        private readonly Dictionary<string, object> logMetadata = new();

        private string hangfireJobId = string.Empty;

        private long wowTokenPrice;

        public PullAuctionDataBackgroundJob(
            DataContext dbContext,
            IBlizzardService blizzardService,
            IEventGridEventSender eventGridEventSender,
            IOptions<BackgroundJobSettings> jobSettings,
            ICorrelationIdService correlationIdService,
            ILogger<PullAuctionDataBackgroundJob> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.eventGridEventSender = eventGridEventSender ?? throw new ArgumentNullException(nameof(eventGridEventSender));
            this.jobSettings = jobSettings?.Value.PullAuctionDataBackgroundJob ?? throw new ArgumentNullException(nameof(jobSettings));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

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
            this.correlationIdService.CorrelationId = $"{this.hangfireJobId}-{Guid.NewGuid()}";
            var numberAuctionEntriesAdded = 0;

            // Can't use tags yet. Issue with Pomelo ef core MySQL connector
            // context.AddTags(nameof(PullAuctionData));

            this.logMetadata[LogMetadataFields.BackgroundJobName] = nameof(PullAuctionDataBackgroundJob);

            this.logger.LogInformation(this.hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullAuctionDataBackgroundJob)} started.", this.logMetadata);

            await this.EnsureWoWTokenItemExists();

            try
            {
                this.wowTokenPrice = (await this.blizzardService.GetWoWTokenPriceAsync()).Price;
            }
            catch (Exception e)
            {
                this.logger.LogError(this.hangfireJobId, sourceName, this.CorrelationId, $"Unable to get WoW token price. {e.Message}");
            }

            try
            {
                this.dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
                this.dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var realmIdsToUpdate = new HashSet<int>();

                if (this.jobSettings.AlwaysProcessCertainItemsEnabled)
                {
                    realmIdsToUpdate.UnionWith(await this.dbContext.ConnectedRealms.AsNoTracking().Select(realm => realm.Id).Distinct().ToListAsync());
                }
                else
                {
                    var realmsWithWatchLists = await this.dbContext.WatchedItems.AsNoTracking().Select(item => item.ConnectedRealmId).Distinct().ToListAsync();
                    realmIdsToUpdate.UnionWith(realmsWithWatchLists);

                    var realmsWithAlerts = await this.dbContext.AlertConditions.AsNoTracking().Select(condition => condition.ConnectedRealmId).Distinct().ToListAsync();
                    realmIdsToUpdate.UnionWith(realmsWithAlerts);
                }

                this.currentItemIds.UnionWith(await this.dbContext.WoWItems.AsNoTracking().Select(i => i.Id).ToListAsync());

                try
                {
                    var newCommodityEntries = await this.ProcessCommoditiesAuctionDataAsync();
                    numberAuctionEntriesAdded += newCommodityEntries;
                }
                catch (Exception e)
                {
                    this.logger.LogError(this.hangfireJobId, sourceName, this.CorrelationId, $"Unable to process commodities. {e.Message}");
                }

                if (this.jobSettings.AlwaysProcessCertainItemsEnabled)
                {
                    var predicate = PredicateBuilder.New<WoWItem>();

                    foreach (var entry in this.jobSettings.AlwayProcessItemClasses)
                    {
                        var itemClass = entry.Key;
                        var itemSubclasses = entry.Value.ToList();
                        predicate.Or(item => item.ItemClass == itemClass && itemSubclasses.Contains(item.ItemSubclass));
                    }

                    this.itemIdsToAlwaysProcess.UnionWith(
                        await this.dbContext.WoWItems.AsNoTracking()
                            .Where(predicate)
                            .Select(item => item.Id).ToListAsync()
                        );
                }

                this.logger.LogInformation(
                    this.hangfireJobId,
                    sourceName,
                    this.CorrelationId,
                    $"Fetched {realmIdsToUpdate.Count} connected realms to update auction data from with {this.itemIdsToAlwaysProcess.Count} items to always process.",
                    this.logMetadata);

                var attempts = new Dictionary<int, int>();
                var realmsQueue = new Queue<int>(realmIdsToUpdate);
                var maxAttempts = 5;

                while (realmsQueue.Any())
                {
                    var connectedRealmId = realmsQueue.Dequeue();

                    if (attempts.ContainsKey(connectedRealmId))
                    {
                        attempts[connectedRealmId]++;
                    }
                    else
                    {
                        attempts[connectedRealmId] = 1;
                    }

                    try
                    {
                        this.logMetadata[nameof(connectedRealmId)] = connectedRealmId;

                        var numAuctionEntriesAdded = await this.ProcessConnectedRealmAuctionDataAsync(connectedRealmId);
                        numberAuctionEntriesAdded += numAuctionEntriesAdded;
                        this.dbContext.ChangeTracker.Clear();
                    }
                    catch (Exception ex)
                    {
                        if (!attempts.ContainsKey(connectedRealmId))
                        {
                            this.logger.LogError(
                                this.hangfireJobId,
                                sourceName,
                                this.CorrelationId,
                                $"Failed to process auction data for connected realm {connectedRealmId} and realm id not in attempts dictionary. Reason: {ex}",
                                this.logMetadata);
                        }

                        var attemptNumber = attempts[connectedRealmId];

                        if (attemptNumber < maxAttempts)
                        {
                            realmsQueue.Enqueue(connectedRealmId);
                            this.logger.LogWarning(
                                this.hangfireJobId,
                                sourceName,
                                this.CorrelationId,
                                $"Failed to process auction data for connected realm {connectedRealmId} after {attemptNumber} attempts. This realm will be retried. Reason: {ex}",
                                this.logMetadata);
                        }
                        else
                        {
                            this.logger.LogError(
                                this.hangfireJobId,
                                sourceName,
                                this.CorrelationId,
                                $"Failed to process auction data for connected realm {connectedRealmId} after {attemptNumber} attempts. No longer retrying. Reason: {ex}",
                                this.logMetadata);
                        }
                    }
                    finally
                    {
                        this.logMetadata.Remove(nameof(connectedRealmId));
                    }
                }

                var numberNewItemsAdded = 0;

                try
                {
                    numberNewItemsAdded = await this.ProcessNewItemsAsync();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(this.hangfireJobId, sourceName, this.CorrelationId, $"Error while processing data for new items. Reason: {ex}", this.logMetadata);
                }

                stopwatch.Stop();
                this.logMetadata[LogMetadataFields.Duration] = stopwatch.ElapsedMilliseconds;
                this.logger.LogInformation(
                    this.hangfireJobId,
                    sourceName,
                    this.CorrelationId,
                    $"{nameof(PullAuctionDataBackgroundJob)} complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries were created. {numberNewItemsAdded} new items added.",
                    this.logMetadata);
            }
            catch (OperationCanceledException ex)
            {
                this.logger.LogWarning(this.hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullAuctionDataBackgroundJob)} canceled. Reason: {ex}", this.logMetadata);
            }
            catch (Exception ex)
            {
                this.logger.LogError(this.hangfireJobId, sourceName, this.CorrelationId, $"{nameof(PullAuctionDataBackgroundJob)} failed. Reason: {ex}", this.logMetadata);
                throw new BackgroundJobClientException(ex.Message, ex);
            }
            finally
            {
                this.dbContext.ChangeTracker.AutoDetectChangesEnabled = true;
                this.dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
        }

        private async Task<int> ProcessConnectedRealmAuctionDataAsync(int connectedRealmId)
        {
            var sourceName = GetSourceName();

            this.logger.LogDebug(this.hangfireJobId, sourceName, this.CorrelationId, $"Processing auction data for connected realm {connectedRealmId}.", this.logMetadata);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var itemsToUpdate = new HashSet<int>(this.itemIdsToAlwaysProcess);

            var itemsInWatchLists = await this.dbContext.WatchedItems
                .Where(item => item.ConnectedRealmId == connectedRealmId)
                .Select(item => item.WoWItemId)
                .Distinct()
                .ToListAsync();
            itemsToUpdate.UnionWith(itemsInWatchLists);

            var itemsInAlerts = await this.dbContext.AlertConditions
                .Where(condition => condition.ConnectedRealmId == connectedRealmId)
                .Select(condition => condition.WoWItemId)
                .Distinct()
                .ToListAsync();
            itemsToUpdate.UnionWith(itemsInAlerts);

            this.logger.LogDebug(
                this.hangfireJobId,
                sourceName, this.CorrelationId,
                $"Determined auction data for {itemsToUpdate.Count} items need to be processed for connected realm {connectedRealmId}.",
                this.logMetadata);

            var auctionData = await this.blizzardService.GetAuctionsAsync(connectedRealmId);

            var newItemIdsFromRealm = auctionData.Auctions
                .Select(auc => auc.Item.Id)
                .Where(id => !this.currentItemIds.Contains(id))
                .ToHashSet();

            this.logger.LogDebug(
                this.hangfireJobId,
                sourceName,
                this.CorrelationId,
                $"Found {newItemIdsFromRealm.Count} untracked items from auction data from connected realm {connectedRealmId}.",
                this.logMetadata);

            this.newItemIds.UnionWith(newItemIdsFromRealm);

            var newAuctionsToAdd = this.MapAuctionData(auctionData.Auctions, connectedRealmId, itemsToUpdate);

            this.dbContext.AuctionTimeSeries.AddRange(newAuctionsToAdd);
            var numberAuctionEntriesAdded = await this.dbContext.SaveChangesAsync();

            await this.eventGridEventSender.SendConnectedRealmAuctionDataUpdateCompleteEventAsync(connectedRealmId);

            stopwatch.Stop();

            this.logger.LogInformation(
                this.hangfireJobId,
                sourceName,
                this.CorrelationId,
                $"Processing auction data for connected realm {connectedRealmId} complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries added and {newItemIdsFromRealm.Count} new items found.",
                this.logMetadata);

            return numberAuctionEntriesAdded;
        }

        private async Task<int> ProcessCommoditiesAuctionDataAsync()
        {
            var sourceName = GetSourceName();

            this.logger.LogDebug(this.hangfireJobId, sourceName, this.CorrelationId, "Processing commodities auction data.", this.logMetadata);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var auctionData = await this.blizzardService.GetCommodityAuctionsAsync();

            var newCommodityItemIds = auctionData.Auctions
                .Select(auc => auc.Item.Id)
                .Where(id => !this.currentItemIds.Contains(id))
                .ToHashSet();

            this.logger.LogDebug(
                this.hangfireJobId,
                sourceName,
                this.CorrelationId,
                $"Found {newCommodityItemIds.Count} untracked items from auction data from commodities data.",
                this.logMetadata);

            this.newItemIds.UnionWith(newCommodityItemIds);

            var newAuctionsToAdd = this.MapAuctionData(auctionData.Auctions, -1, new HashSet<int>(), true);

            this.dbContext.AuctionTimeSeries.AddRange(newAuctionsToAdd);
            var numberAuctionEntriesAdded = await this.dbContext.SaveChangesAsync();

            stopwatch.Stop();

            this.logger.LogInformation(
                this.hangfireJobId,
                sourceName,
                this.CorrelationId,
                $"Processing commodity auction data complete in {stopwatch.ElapsedMilliseconds}ms. {numberAuctionEntriesAdded} auction entries added and {newCommodityItemIds.Count} new items found.",
                this.logMetadata);

            return numberAuctionEntriesAdded;
        }

        private async Task EnsureWoWTokenItemExists()
        {
            var wowToken = await this.dbContext.WoWItems.FindAsync(ApplicationSettings.WoWTokenId);

            if (wowToken != null)
            {
                return;
            }

            var wowTokenItem = await this.blizzardService.GetWoWItemAsync(ApplicationSettings.WoWTokenId);

            this.dbContext.WoWItems.Add(new WoWItem
            {
                Id = wowTokenItem.Id,
                Name = wowTokenItem.Name,
                IsEquippable = wowTokenItem.IsEquippable,
                IsStackable = wowTokenItem.IsStackable,
                Level = wowTokenItem.Level,
                RequiredLevel = wowTokenItem.RequiredLevel,
                SellPrice = wowTokenItem.SellPrice,
                PurchaseQuantity = wowTokenItem.PurchaseQuantity,
                PurchasePrice = wowTokenItem.PurchasePrice,
                ItemClass = wowTokenItem.ItemClass.Name,
                ItemSubclass = wowTokenItem.ItemSubclass.Name,
                Quality = "WoWToken",
                InventoryType = wowTokenItem.InventoryType.Name,
                MaxCount = wowTokenItem.MaxCount
            });

            await this.dbContext.SaveChangesAsync();
        }

        private async Task<int> ProcessNewItemsAsync()
        {
            var sourceName = GetSourceName();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            this.logger.LogDebug(this.hangfireJobId, sourceName, this.CorrelationId, $"Starting to obtain and save data for {this.newItemIds.Count} newly discovered items.", this.logMetadata);

            var newItemChunks = this.newItemIds.Chunk(100);

            var newItemChunkedChunks = newItemChunks.Chunk(5);

            var tasks = new List<Task<IEnumerable<WoWItem>>>();

            foreach (var chunkedChunk in newItemChunkedChunks)
            {
                tasks.Add(this.HandleChunkedItemRequestsAsync(chunkedChunk));
            }

            var itemsFromBlizzard = (await Task.WhenAll(tasks)).SelectMany(item => item);

            this.dbContext.WoWItems.AddRange(itemsFromBlizzard);

            var itemsSaved = await this.dbContext.SaveChangesAsync();

            stopwatch.Stop();
            this.logger.LogDebug(
                this.hangfireJobId,
                sourceName, this.CorrelationId,
                $"Obtaining and saving data for {this.newItemIds.Count} newly discovered items complete in {stopwatch.ElapsedMilliseconds}ms. {itemsSaved} database records created.",
                this.logMetadata);

            return itemsSaved;
        }

        private List<AuctionTimeSeriesEntry> MapAuctionData(List<BlizzardAuction> auctions, int connectedRealmId, HashSet<int> itemIdsToProcess, bool processAllAuctions = false)
        {
            var sourceName = GetSourceName();

            var itemIdAuctionMap = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in auctions)
            {
                if (!processAllAuctions && (!itemIdsToProcess.Contains(auction.Item.Id) || !this.currentItemIds.Contains(auction.Item.Id)))
                {
                    continue;
                }

                var price = auction.Buyout ?? auction.UnitPrice ?? auction.Bid;

                if (price == null)
                {
                    this.logger.LogWarning(this.hangfireJobId, sourceName, this.CorrelationId, $"Auction {auction.Id} does not have a buyout or unit price.", this.logMetadata);
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

            var auctionEntries = itemIdAuctionMap.Values.ToList();

            // Always add wow token price if we were able to obtain it.
            if (this.wowTokenPrice != default)
            {
                auctionEntries.Add(new AuctionTimeSeriesEntry
                {
                    WoWItemId = ApplicationSettings.WoWTokenId,
                    ConnectedRealmId = connectedRealmId,
                    Timestamp = utcNow,
                    TotalAvailableForAuction = 1,
                    AveragePrice = this.wowTokenPrice,
                    MinPrice = this.wowTokenPrice,
                    MaxPrice = this.wowTokenPrice,
                    Price25Percentile = this.wowTokenPrice,
                    Price50Percentile = this.wowTokenPrice,
                    Price75Percentile = this.wowTokenPrice,
                    Price95Percentile = this.wowTokenPrice,
                    Price99Percentile = this.wowTokenPrice
                });
            }

            return auctionEntries;
        }

        private async Task<IEnumerable<WoWItem>> HandleChunkedItemRequestsAsync(IEnumerable<IEnumerable<int>> chunkedItemIds)
        {
            var sourceName = GetSourceName();
            var result = new List<WoWItem>();

            foreach (var chunk in chunkedItemIds)
            {
                try
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
                catch (Exception ex)
                {
                    this.logger.LogError(this.hangfireJobId, sourceName, this.CorrelationId, $"Failed to handle chunk of {chunk.Count()} items. Reason: {ex}", this.logMetadata);
                }
            }

            return result;
        }
    }
}