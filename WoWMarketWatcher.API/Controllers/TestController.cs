using System.Threading;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using Microsoft.AspNetCore.Authorization;
using WoWMarketWatcher.API.Services;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using System;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.Common.Extensions;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ServiceControllerBase
    {
        private readonly Counter counter;
        private readonly IBlizzardService blizzardService;
        private readonly ILogger<TestController> logger;

        public TestController(Counter counter, IBlizzardService blizzardService, ILogger<TestController> logger)
        {
            this.counter = counter;
            this.blizzardService = blizzardService;
            this.logger = logger;
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
            var sourceName = this.GetSourceName();
            var correlationId = Guid.NewGuid().ToString();
            var LOL = "foobar";

            this.logger.LogInformation(sourceName, correlationId, "TEST {LOL}", "I {intend to break things}");

            return this.Ok(new { correlationId });
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