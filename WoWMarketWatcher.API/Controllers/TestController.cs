using System.Threading;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using Microsoft.AspNetCore.Authorization;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using System.Threading.Tasks;
using System.Collections.Generic;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Entities;
using System;
using WoWMarketWatcher.API.Extensions;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ServiceControllerBase
    {
        private readonly Counter counter;

        private readonly BlizzardService blizzardService;

        private readonly DataContext db;

        public TestController(Counter counter, BlizzardService blizzardService, DataContext db)
        {
            this.counter = counter;
            this.blizzardService = blizzardService;
            this.db = db;
        }

        [HttpPost("blizz/auctions")]
        public async Task<ActionResult> TestBlizzardAuctionsInsertAsync()
        {
            var currentItemsTask = db.WoWItems.Select(i => i.Id).ToListAsync();
            var res = await this.blizzardService.GetAuctionsAsync(3694);

            var currentItems = (await currentItemsTask).ToHashSet();

            var dict = new Dictionary<int, AuctionTimeSeriesEntry>();
            var seen = new Dictionary<int, List<(long amount, long price)>>();

            var utcNow = DateTime.UtcNow;

            foreach (var auction in res.Auctions)
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

            var newItems = dict.Keys.Where(k => !currentItems.Contains(k));

            var chunks = newItems.ChunkBy(100);

            var chunkedChunks = chunks.ChunkBy(5);

            var tasks = new List<Task<IEnumerable<WoWItem>>>();

            foreach (var chunkedChunk in chunkedChunks)
            {
                tasks.Add(this.HandleChunkAsync(chunkedChunk));
            }

            var items = (await Task.WhenAll(tasks)).SelectMany(x => x);

            currentItems.UnionWith(items.Select(i => i.Id));

            db.WoWItems.AddRange(items);

            db.AuctionTimeSeries.AddRange(dict.Values.Where(e => currentItems.Contains(e.WoWItemId)));

            await db.SaveChangesAsync();

            var notAdded = dict.Values.Where(e => !currentItems.Contains(e.WoWItemId));

            // var jsonString = JsonConvert.SerializeObject(items);

            // await System.IO.File.WriteAllTextAsync("wowItems.json", jsonString);

            // var jsonString = JsonConvert.SerializeObject(dict.Values.Take(100));

            // await System.IO.File.WriteAllTextAsync("AuctionTimeSeriesSeedData.json", jsonString);

            return Ok(notAdded);
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

        [HttpGet("blizz/token")]
        public async Task<ActionResult<BlizzardTokenResponse>> TestBlizzardTokenAsync()
        {
            var res = await this.blizzardService.GetAccessTokenAsync();

            return Ok(new { accessToken = res });
        }

        [HttpGet("blizz/auctions")]
        public async Task<ActionResult<BlizzardAuctionsResponse>> TestBlizzardAuctionsAsync()
        {
            var res = await this.blizzardService.GetAuctionsAsync(3694);

            return Ok(res);
        }

        [HttpGet("blizz/realms")]
        public async Task<ActionResult<BlizzardSearchResponse<BlizzardConnectedRealm>>> TestBlizzardRealmsAsync()
        {
            var res = await this.blizzardService.GetConnectedRealmsAsync();

            return Ok(res);
        }

        [HttpPost("blizz/realms")]
        public async Task<ActionResult<BlizzardSearchResponse<BlizzardConnectedRealm>>> TestBlizzardRealmsPostAsync()
        {
            var res = await this.blizzardService.GetConnectedRealmsAsync();

            var connectedRealms = res.Results.Select(r => r.Data).Select(r => new ConnectedRealm
            {
                Id = r.Id,
                Realms = r.Realms.Select(realm => new Realm
                {
                    Id = realm.Id,
                    ConnectedRealmId = r.Id,
                    Name = realm.Name.EnUS,
                    IsTournament = realm.IsTournament,
                    Locale = realm.Locale,
                    Timezone = realm.Timezone,
                    Slug = realm.Slug,
                    Region = realm.Region.Name.EnUS,
                    Category = realm.Category.EnUS,
                    Type = realm.Type.Name.EnUS
                }).ToList()
            });

            db.ConnectedRealms.AddRange(connectedRealms);

            var jsonString = JsonConvert.SerializeObject(connectedRealms);

            await System.IO.File.WriteAllTextAsync("ConnectedRealmsSeedData.json", jsonString);

            await db.SaveChangesAsync();

            return Ok(res);
        }

        [HttpGet("blizz/items/search")]
        public async Task<ActionResult<BlizzardSearchResponse<BlizzardLocaleWoWItem>>> TestBlizzardItemsSearchAsync()
        {
            var auctions = await this.blizzardService.GetAuctionsAsync(3694);

            var ids = auctions.Auctions.Select(a => a.Item.Id).ToHashSet().Take(100);

            var res = await this.blizzardService.GetWoWItemsAsync(ids);

            return Ok(res);
        }

        [HttpGet]
        public ActionResult Test([FromQuery] HttpStatusCode status, [FromQuery] HttpStatusCode? statusAfter, [FromQuery] int? per, [FromQuery] int delay = 0)
        {
            this.counter.Count++;
            Thread.Sleep(delay);

            return statusAfter != null && per != null && per.Value != 0 && this.counter.Count % per.Value == 0
                ? this.GetReturn(statusAfter.Value)
                : this.GetReturn(status);
        }

        private ActionResult GetReturn(HttpStatusCode status)
        {
            return status switch
            {
                HttpStatusCode.OK => Ok(new { message = "Test ok" }),
                HttpStatusCode.BadRequest => BadRequest("Test Bad Request"),
                HttpStatusCode.Unauthorized => Unauthorized("Test unauthorized"),
                HttpStatusCode.InternalServerError => InternalServerError("Test internal server error"),
                HttpStatusCode.BadGateway => StatusCode(502, new ProblemDetailsWithErrors("Test bad gateway", 502, Request)),
                HttpStatusCode.ServiceUnavailable => StatusCode(503, new ProblemDetailsWithErrors("Test service not available", 503, Request)),
                HttpStatusCode.GatewayTimeout => StatusCode(504, new ProblemDetailsWithErrors("Test timeout", 504, Request)),
                _ => Ok(new { message = "default" }),
            };
        }
    }
}