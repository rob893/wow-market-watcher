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
    [Route("api/v{version:apiVersion}/wow/connectedRealms")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class ConnectedRealmsController : ServiceControllerBase
    {
        private readonly IConnectedRealmRepository connectedRealmRepository;

        private readonly IMapper mapper;

        public ConnectedRealmsController(IConnectedRealmRepository connectedRealmRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.connectedRealmRepository = connectedRealmRepository ?? throw new ArgumentNullException(nameof(connectedRealmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a paginated list of connected realms matching the seach critera.
        /// </summary>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of connected realms matching the seach critera.</returns>
        /// <response code="200">A paginated list of connected realms.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet(Name = nameof(GetConnectedRealmsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<ConnectedRealmDto>>> GetConnectedRealmsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
        {
            var realms = await this.connectedRealmRepository.SearchAsync(searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<ConnectedRealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        /// <summary>
        /// Gets a single connected realm by id.
        /// </summary>
        /// <param name="id">The id of the connected realm.</param>
        /// <returns>A single connected realm if found.</returns>
        /// <response code="200">The connected realm.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}", Name = nameof(GetConnectedRealmAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConnectedRealmDto>> GetConnectedRealmAsync([FromRoute] int id)
        {
            var connectedRealm = await this.connectedRealmRepository.GetByIdAsync(id, false);

            if (connectedRealm == null)
            {
                return this.NotFound($"Connected realm with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<ConnectedRealmDto>(connectedRealm);

            return this.Ok(mapped);
        }

        /// <summary>
        /// Gets a paginated list of realms for a connected realm matching the seach critera.
        /// </summary>
        /// <param name="id">The id of the connected realm.</param>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of realms matching the seach critera.</returns>
        /// <response code="200">A paginated list of realms.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}/realms", Name = nameof(GetRealmsForConnectedRealmAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<RealmDto>>> GetRealmsForConnectedRealmAsync([FromRoute] int id, [FromQuery] RealmQueryParameters searchParams)
        {
            var connectedRealm = await this.connectedRealmRepository.GetByIdAsync(id, false);

            if (connectedRealm == null)
            {
                return this.NotFound($"Connected realm with id {id} does not exist.");
            }

            var realms = await this.connectedRealmRepository.GetRealmsForConnectedRealmAsync(connectedRealm.Id, searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<RealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }
    }
}