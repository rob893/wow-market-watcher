using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Requests.WatchLists;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/watchLists")]
    [ApiVersion("1")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ApiController]
    public sealed class WatchListsController : ServiceControllerBase
    {
        private readonly IWatchListRepository watchListRepository;

        private readonly IMapper mapper;

        public WatchListsController(IWatchListRepository watchListRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.watchListRepository = watchListRepository ?? throw new ArgumentNullException(nameof(watchListRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets a paginated list of watch lists matching the seach critera.
        /// </summary>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of watch lists matching the seach critera.</returns>
        /// <response code="200">A paginated list of watch lists.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet(Name = nameof(GetWatchListsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<WatchListDto>>> GetWatchListsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
        {
            var lists = await this.watchListRepository.SearchAsync(searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<WatchListDto>>(lists.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        /// <summary>
        /// Gets a single watch list by id.
        /// </summary>
        /// <param name="id">The id of the watch list.</param>
        /// <returns>A single watch list if found.</returns>
        /// <response code="200">The watch list.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}", Name = nameof(GetWatchListAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WatchListDto>> GetWatchListAsync([FromRoute] int id)
        {
            var list = await this.watchListRepository.GetByIdAsync(id, false);

            if (list == null)
            {
                return this.NotFound($"Watch list with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<WatchListDto>(list);

            return this.Ok(mapped);
        }

        /// <summary>
        /// Creates a new watch list.
        /// </summary>
        /// <param name="request">The create request.</param>
        /// <returns>The newly created watch list.</returns>
        /// <response code="201">The newly created watch list.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost(Name = nameof(CreateWatchListAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<WatchListDto>> CreateWatchListAsync([FromBody] CreateWatchListRequest request)
        {
            var newWatchList = this.mapper.Map<WatchList>(request);
            this.watchListRepository.Add(newWatchList);

            var saveResult = await this.watchListRepository.SaveAllAsync();

            if (!saveResult)
            {
                return this.BadRequest("Unable to create watch list.");
            }

            var mapped = this.mapper.Map<WatchListDto>(newWatchList);

            return this.CreatedAtRoute(nameof(GetWatchListAsync), new { id = mapped.Id }, mapped);
        }

        /// <summary>
        /// Deletes a single watch list by id.
        /// </summary>
        /// <param name="id">The id of the watch list.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the resource was deleted.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpDelete("{id}", Name = nameof(DeleteWatchListAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteWatchListAsync([FromRoute] int id)
        {
            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            this.watchListRepository.Remove(watchList);
            var saveResults = await this.watchListRepository.SaveAllAsync();

            return !saveResults ? this.BadRequest("Failed to delete the income.") : this.NoContent();
        }

        /// <summary>
        /// Updates a single watch list by id.
        /// </summary>
        /// <param name="id">The id of the watch list.</param>
        /// <param name="requestPatchDoc">The update request.</param>
        /// <returns>The updated watch list.</returns>
        /// <response code="200">If the watch list was updated.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPatch("{id}", Name = nameof(UpdateWatchListAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WatchListDto>> UpdateWatchListAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateWatchListRequest> requestPatchDoc)
        {
            if (requestPatchDoc == null || requestPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No watch list with Id {id} found.");
            }

            if (!requestPatchDoc.IsValid(out var errors))
            {
                return this.BadRequest(errors);
            }

            var patchDoc = this.mapper.Map<JsonPatchDocument<WatchList>>(requestPatchDoc);

            patchDoc.ApplyTo(watchList);

            await this.watchListRepository.SaveAllAsync();

            var mapped = this.mapper.Map<WatchListDto>(watchList);

            return this.Ok(mapped);
        }
    }
}