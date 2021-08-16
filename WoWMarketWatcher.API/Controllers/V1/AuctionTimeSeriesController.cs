using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/wow/auctionTimeSeries")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class AuctionTimeSeriesController : ServiceControllerBase
    {
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;

        private readonly IMapper mapper;

        public AuctionTimeSeriesController(IAuctionTimeSeriesRepository timeSeriesRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a paginated list of auction time series matching the seach critera.
        /// </summary>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of auction time series matching the seach critera.</returns>
        /// <response code="200">The auction time series.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet(Name = nameof(GetAuctionTimeSeriesAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>> GetAuctionTimeSeriesAsync([FromQuery] AuctionTimeSeriesQueryParameters searchParams)
        {
            var timeSeriesEntries = await this.timeSeriesRepository.SearchAsync(searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>(timeSeriesEntries.ToCursorPaginatedResponse(entry => entry.Id, searchParams));

            return this.Ok(paginatedResponse);
        }

        /// <summary>
        /// Gets a single auction time series entry by id.
        /// </summary>
        /// <param name="id">The id of the entry.</param>
        /// <returns>A single auction time series entry if found.</returns>
        /// <response code="200">The auction time series.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}", Name = nameof(GetAuctionTimeSeriesEntryAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuctionTimeSeriesEntryDto>> GetAuctionTimeSeriesEntryAsync([FromRoute] int id)
        {
            var entry = await this.timeSeriesRepository.GetByIdAsync(id, false);

            if (entry == null)
            {
                return this.NotFound($"Time series entry with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<AuctionTimeSeriesEntryDto>(entry);

            return this.Ok(mapped);
        }
    }
}