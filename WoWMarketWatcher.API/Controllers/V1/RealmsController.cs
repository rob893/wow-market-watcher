using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs.Realms;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/wow/realms")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class RealmsController : ServiceControllerBase
    {
        private readonly IRealmRepository realmRepository;

        private readonly IMapper mapper;

        public RealmsController(IRealmRepository realmRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a paginated list of realms matching the seach critera.
        /// </summary>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of realms matching the seach critera.</returns>
        /// <response code="200">A paginated list of realms.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet(Name = nameof(GetRealmsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<RealmDto>>> GetRealmsAsync([FromQuery] RealmQueryParameters searchParams)
        {
            var realms = await this.realmRepository.SearchAsync(searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<RealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        /// <summary>
        /// Gets a single realm by id.
        /// </summary>
        /// <param name="id">The id of the realm.</param>
        /// <returns>A single realm if found.</returns>
        /// <response code="200">The realm.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}", Name = nameof(GetRealmAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RealmDto>> GetRealmAsync([FromRoute] int id)
        {
            var realm = await this.realmRepository.GetByIdAsync(id, false);

            if (realm == null)
            {
                return this.NotFound($"Realm with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<RealmDto>(realm);

            return this.Ok(mapped);
        }
    }
}