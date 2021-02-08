using System.Threading;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using Microsoft.AspNetCore.Authorization;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ServiceControllerBase
    {
        private readonly Counter counter;

        public TestController(Counter counter)
        {
            this.counter = counter;
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