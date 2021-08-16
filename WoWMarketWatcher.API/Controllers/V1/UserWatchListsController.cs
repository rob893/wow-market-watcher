using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/v{version:apiVersion}/users/{userId}/watchLists")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class UserWatchListsController : ServiceControllerBase
    {
        private readonly IWatchListRepository watchListRepository;

        private readonly IWoWItemRepository itemRepository;

        private readonly IConnectedRealmRepository realmRepository;

        private readonly IMapper mapper;

        public UserWatchListsController(
            IWatchListRepository watchListRepository,
            IWoWItemRepository itemRepository,
            IConnectedRealmRepository realmRepository,
            IMapper mapper,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.watchListRepository = watchListRepository ?? throw new ArgumentNullException(nameof(watchListRepository));
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet(Name = nameof(GetWatchListsForUserAsync))]
        public async Task<ActionResult<CursorPaginatedResponse<WatchListDto>>> GetWatchListsForUserAsync([FromRoute] int userId, [FromQuery] CursorPaginationQueryParameters searchParams)
        {
            if (!this.IsUserAuthorizedForResource(userId))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var lists = await this.watchListRepository.GetWatchListsForUserAsync(userId, searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<WatchListDto>>(lists.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = nameof(GetWatchListForUserAsync))]
        public async Task<ActionResult<WatchListDto>> GetWatchListForUserAsync([FromRoute] int id)
        {
            var list = await this.watchListRepository.GetByIdAsync(id, false);

            if (list == null)
            {
                return this.NotFound($"Watch list with id {id} does not exist.");
            }

            if (!this.IsUserAuthorizedForResource(list))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var mapped = this.mapper.Map<WatchListDto>(list);

            return this.Ok(mapped);
        }

        [HttpPost(Name = nameof(CreateWatchListForUserAsync))]
        public async Task<ActionResult<WatchListDto>> CreateWatchListForUserAsync([FromRoute] int userId, [FromBody] CreateWatchListForUserRequest request)
        {
            if (!this.IsUserAuthorizedForResource(userId))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var newWatchList = this.mapper.Map<WatchList>(request);
            newWatchList.UserId = userId;

            var realm = await this.realmRepository.GetByIdAsync(newWatchList.ConnectedRealmId);

            if (realm == null)
            {
                return this.BadRequest($"No connected realm with id ${newWatchList.ConnectedRealmId} exists.");
            }

            this.watchListRepository.Add(newWatchList);

            var saveResult = await this.watchListRepository.SaveAllAsync();

            if (!saveResult)
            {
                return this.BadRequest("Unable to create watch list.");
            }

            var mapped = this.mapper.Map<WatchListDto>(newWatchList);

            return this.CreatedAtRoute("GetWatchListForUserAsync", new { id = mapped.Id, userId }, mapped);
        }

        [HttpDelete("{id}", Name = nameof(DeleteWatchListForUserAsync))]
        public async Task<ActionResult> DeleteWatchListForUserAsync([FromRoute] int id)
        {
            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(watchList))
            {
                return this.Forbidden("You are not authorized to delete this resource.");
            }

            this.watchListRepository.Remove(watchList);
            var saveResults = await this.watchListRepository.SaveAllAsync();

            return saveResults ? this.NoContent() : this.BadRequest("Failed to delete the resource.");
        }

        [HttpPatch("{id}", Name = nameof(UpdateWatchListForUserAsync))]
        public async Task<ActionResult<WatchListDto>> UpdateWatchListForUserAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateWatchListRequest> requestPatchDoc)
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

            if (!this.IsUserAuthorizedForResource(watchList))
            {
                return this.Forbidden("You are not authorized to update this resource.");
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

        [HttpPost("{id}/items", Name = nameof(AddItemToWatchListForUserAsync))]
        public async Task<ActionResult<WatchListDto>> AddItemToWatchListForUserAsync([FromRoute] int id, [FromBody] AddItemToWatchListRequest request)
        {
            if (request == null || request.Id == null)
            {
                return this.BadRequest($"Id is required.");
            }

            var watchList = await this.watchListRepository.GetByIdAsync(id, list => list.WatchedItems);

            if (watchList == null)
            {
                return this.NotFound($"No watch list with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(watchList))
            {
                return this.Forbidden("You are not authorized to change this resource.");
            }

            if (watchList.WatchedItems.FirstOrDefault(watchedItem => watchedItem.Id == request.Id) != null)
            {
                return this.BadRequest($"Watch list {watchList.Id} is already watching item {request.Id}.");
            }

            var itemToAdd = await this.itemRepository.GetByIdAsync(request.Id.Value);

            if (itemToAdd == null)
            {
                return this.NotFound($"No item with Id {request.Id.Value} found.");
            }

            watchList.WatchedItems.Add(itemToAdd);

            var saveResults = await this.watchListRepository.SaveAllAsync();

            if (!saveResults)
            {
                return this.BadRequest("Failed to add the item to watch list.");
            }

            var mapped = this.mapper.Map<WatchListDto>(watchList);

            return this.Ok(mapped);
        }

        [HttpDelete("{id}/items/{itemId}", Name = nameof(RemoveItemFromWatchListForUserAsync))]
        public async Task<ActionResult<WatchListDto>> RemoveItemFromWatchListForUserAsync([FromRoute] int id, [FromRoute] int itemId)
        {
            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(watchList))
            {
                return this.Forbidden("You are not authorized to delete this resource.");
            }

            var itemToRemove = watchList.WatchedItems.FirstOrDefault(item => item.Id == itemId);

            if (itemToRemove == null)
            {
                return this.BadRequest($"Watch list {id} does not have item {itemId}.");
            }

            watchList.WatchedItems.Remove(itemToRemove);

            var saveResults = await this.watchListRepository.SaveAllAsync();

            if (!saveResults)
            {
                return this.BadRequest("Failed to delete the resource.");
            }

            var mapped = this.mapper.Map<WatchListDto>(watchList);

            return this.Ok(mapped);
        }
    }
}