using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Test endpoint for v2 API.
        /// </summary>
        /// <returns>YAY!!!</returns>
        /// <response code="200">The user object and tokens.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("login", Name = nameof(LoginAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<string> LoginAsync()
        {
            return this.Ok("YAY!!!");
        }
    }
}