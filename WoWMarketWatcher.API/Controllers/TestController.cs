using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.Common.Constants;
using WoWMarketWatcher.Common.Extensions;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    public class TestController : ServiceControllerBase
    {
        private readonly Counter counter;
        private readonly IBlizzardService blizzardService;
        private readonly ILogger<TestController> logger;
        private readonly DataContext dbContext;
        private readonly IHttpClientFactory httpClientFactory;

        public TestController(Counter counter, IBlizzardService blizzardService, ILogger<TestController> logger, DataContext dbContext, IHttpClientFactory httpClientFactory)
        {
            this.counter = counter;
            this.blizzardService = blizzardService;
            this.logger = logger;
            this.dbContext = dbContext;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("blizz/items/{id}")]
        public async Task<ActionResult<BlizzardWoWItem>> GetItem([FromRoute] int id)
        {
            var item = await this.blizzardService.GetWoWItemAsync(id, Guid.NewGuid().ToString());

            return this.Ok(item);
        }

        [HttpGet("logger")]
        public ActionResult<BlizzardWoWItem> TestLogger()
        {
            var sourceName = GetSourceName();
            var correlationId = Guid.NewGuid().ToString();

            this.logger.LogInformation(sourceName, correlationId, $"TEST ASDF");

            return this.Ok(new { correlationId });
        }

        [HttpPost("auctionTimeSeries/download")]
        public async Task<ActionResult> DownloadAuctionTimeSeriesAsync()
        {
            var items = await this.dbContext.AuctionTimeSeries.ToListAsync();

            var asJson = items.ToJson();

            await System.IO.File.WriteAllTextAsync($"Data/SeedData/AuctionTimeSeriesSeedData-{DateTime.Now:dd-MM-yy}.json", asJson);

            return this.Ok();
        }

        [HttpPost("wowItems/download")]
        public async Task<ActionResult> DownloadWoWItemsAsync()
        {
            var items = await this.dbContext.WoWItems.ToListAsync();

            var asJson = items.ToJson();

            await System.IO.File.WriteAllTextAsync($"Data/SeedData/WoWItemsSeedData-{DateTime.Now:dd-MM-yy}.json", asJson);

            return this.Ok();
        }

        [HttpGet("self")]
        [AllowAnonymous]
        public async Task<ActionResult> TestSelf([FromQuery] HttpStatusCode status, [FromQuery] HttpStatusCode? statusAfter, [FromQuery] int? per, [FromQuery] int delay = 0)
        {
            var query = $"?status={status}&delay={delay}{(statusAfter == null ? "" : $"&statusAfter={statusAfter}")}{(per == null ? "" : $"&per={per}")}";

            var client = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            var res = await client.GetAsync($"http://localhost:5003/api/test{query}");

            return this.Ok();
        }

        [HttpGet]
        [AllowAnonymous]
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
                HttpStatusCode.OK => this.Ok(new { message = "Test ok" }),
                HttpStatusCode.BadRequest => this.BadRequest("Test Bad Request"),
                HttpStatusCode.Unauthorized => this.Unauthorized("Test unauthorized"),
                HttpStatusCode.InternalServerError => this.InternalServerError("Test internal server error"),
                HttpStatusCode.BadGateway => this.StatusCode(502, new ProblemDetailsWithErrors("Test bad gateway", 502, this.Request)),
                HttpStatusCode.ServiceUnavailable => this.StatusCode(503, new ProblemDetailsWithErrors("Test service not available", 503, this.Request)),
                HttpStatusCode.GatewayTimeout => this.StatusCode(504, new ProblemDetailsWithErrors("Test timeout", 504, this.Request)),
                _ => this.Ok(new { message = "default" }),
            };
        }
    }
}