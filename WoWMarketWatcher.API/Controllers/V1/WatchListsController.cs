using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
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

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<WatchListDto>>> GetWatchListsAsync([FromQuery] RealmQueryParameters searchParams)
        {
            var lists = await this.watchListRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<WatchListDto>>(lists.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetWatchListAsync")]
        public async Task<ActionResult<WatchListDto>> GetWatchListAsync([FromRoute] int id)
        {
            var list = await this.watchListRepository.GetByIdAsync(id);

            if (list == null)
            {
                return this.NotFound($"Watch list with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<WatchListDto>(list);

            return this.Ok(mapped);
        }

        [HttpPost]
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

            return this.CreatedAtRoute("GetWatchListAsync", new { id = mapped.Id }, mapped);
        }

        [HttpDelete("{id}")]
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

        [HttpPatch("{id}")]
        public async Task<ActionResult<WatchListDto>> UpdateWatchListAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateWatchListRequest> requestPatchDoc)
        {
            if (requestPatchDoc == null || requestPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No expense with Id {id} found.");
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