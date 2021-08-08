using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V2
{
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2")]
    [ApiController]
    public sealed class AuthController : ServiceControllerBase
    {
        public AuthController(ICorrelationIdService correlationIdService) : base(correlationIdService) { }

        [HttpPost("login")]
        public ActionResult<string> LoginAsync()
        {
            return this.Ok("YAY!!!");
        }
    }
}