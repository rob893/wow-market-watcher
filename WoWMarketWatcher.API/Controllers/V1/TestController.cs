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
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Data;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Services;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/test")]
    [ApiVersion("1")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ApiController]
    public sealed class TestController : ServiceControllerBase
    {
        private readonly Counter counter;

        private readonly IBlizzardService blizzardService;

        private readonly ILogger<TestController> logger;

        private readonly DataContext dbContext;

        private readonly IAlertService alertService;

        private readonly IHttpClientFactory httpClientFactory;

        public TestController(
            Counter counter,
            IBlizzardService blizzardService,
            ILogger<TestController> logger,
            DataContext dbContext,
            IAlertService alertService,
            IHttpClientFactory httpClientFactory,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.counter = counter ?? throw new ArgumentNullException(nameof(counter));
            this.blizzardService = blizzardService ?? throw new ArgumentNullException(nameof(blizzardService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        [HttpPost("alerts/evaluate")]
        public async Task<ActionResult<int>> EvaludateAlertsAsync()
        {
            var alerts = await this.dbContext.Alerts
                .Include(alert => alert.Actions)
                .Include(alert => alert.Conditions)
                .ToListAsync();

            var alertsTriggered = 0;

            foreach (var alert in alerts)
            {
                var fired = await this.alertService.EvaluateAlertAsync(alert);

                if (fired)
                {
                    alertsTriggered++;
                }
            }

            return this.Ok(alertsTriggered);
        }

        [HttpGet("blizz/items/{id}")]
        public async Task<ActionResult<BlizzardWoWItem>> GetItem([FromRoute] int id)
        {
            var item = await this.blizzardService.GetWoWItemAsync(id);

            return this.Ok(item);
        }

        [HttpGet("blizz/wowTokenPrice")]
        public async Task<ActionResult<BlizzardWoWTokenResponse>> GetWowToken()
        {
            var token = await this.blizzardService.GetWoWTokenPriceAsync();

            return this.Ok(token);
        }

        [HttpGet("misc")]
        public async Task<ActionResult> GetMisc()
        {
            var items = await this.blizzardService.GetAllConnectedRealmsAsync();

            return this.Ok(items);
        }

        [HttpGet("logger")]
        public ActionResult<BlizzardWoWItem> TestLogger()
        {
            var sourceName = GetSourceName();

            this.logger.LogInformation(sourceName, this.CorrelationId, $"TEST ASDF");

            return this.Ok(new { this.CorrelationId });
        }

        [HttpPost("auctionTimeSeries/download")]
        public async Task<ActionResult> DownloadAuctionTimeSeriesAsync()
        {
            var items = await this.dbContext.AuctionTimeSeries.ToListAsync();

            var asJson = items.ToJson();

            await System.IO.File.WriteAllTextAsync($"Data/SeedData/AuctionTimeSeriesSeedData-{DateTime.Now:dd-MM-yy}.json", asJson);

            return this.Ok(items);
        }

        [HttpPost("wowItems/download")]
        public async Task<ActionResult> DownloadWoWItemsAsync()
        {
            var items = await this.dbContext.WoWItems.ToListAsync();

            var asJson = items.ToJson();

            await System.IO.File.WriteAllTextAsync($"Data/SeedData/WoWItemsSeedData-{DateTime.Now:dd-MM-yy}.json", asJson);

            return this.Ok(items);
        }

        [HttpPost("connectedRealms/download")]
        public async Task<ActionResult> DownloadConnectedRealmsAsync()
        {
            var items = await this.dbContext.ConnectedRealms.Include(r => r.Realms).ToListAsync();

            var asJson = items.ToJson();

            await System.IO.File.WriteAllTextAsync($"Data/SeedData/ConnectedRealmsSeedData-{DateTime.Now:dd-MM-yy}.json", asJson);

            return this.Ok(items);
        }

        [HttpGet("self")]
        [AllowAnonymous]
        public async Task<ActionResult> TestSelf([FromQuery] HttpStatusCode status, [FromQuery] HttpStatusCode? statusAfter, [FromQuery] int? delayAfter, [FromQuery] int? delayPer, [FromQuery] int? per, [FromQuery] int delay = 0)
        {
            var query = $"?status={status}&delay={delay}{(statusAfter == null ? "" : $"&statusAfter={statusAfter}")}{(per == null ? "" : $"&per={per}")}{(delayPer == null ? "" : $"&delayPer={delayPer}")}{(delayAfter == null ? "" : $"&delayAfter={delayAfter}")}";

            var client = this.httpClientFactory.CreateClient(nameof(BlizzardService));

            var res = await client.GetAsync(new Uri($"http://localhost:5003/api/test{query}"));

            return this.Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Test([FromQuery] HttpStatusCode status, [FromQuery] HttpStatusCode? statusAfter, [FromQuery] int? delayAfter, [FromQuery] int? delayPer, [FromQuery] int? per, [FromQuery] int delay = 0)
        {
            this.counter.Count++;
            Thread.Sleep(delayAfter != null && delayPer != null && this.counter.Count % delayPer.Value == 0 ? delayAfter.Value : delay);

            return statusAfter != null && per != null && per.Value != 0 && this.counter.Count % per.Value == 0
                ? this.GetReturn(statusAfter.Value)
                : this.GetReturn(status);
        }

        [HttpPost("resetCounter")]
        [AllowAnonymous]
        public ActionResult ResetCounter()
        {
            this.counter.Count = 0;
            return this.Ok();
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