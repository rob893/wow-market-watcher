using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Requests;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using System.Linq;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/users/{userId}/watchLists")]
    [ApiController]
    public class UserWatchListsController : ServiceControllerBase
    {
        private readonly WatchListRepository watchListRepository;
        private readonly WoWItemRepository itemRepository;
        private readonly IMapper mapper;


        public UserWatchListsController(WatchListRepository watchListRepository, WoWItemRepository itemRepository, IMapper mapper)
        {
            this.watchListRepository = watchListRepository;
            this.itemRepository = itemRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<WatchListDto>>> GetWatchListsForUserAsync([FromRoute] int userId, [FromQuery] RealmQueryParameters searchParams)
        {
            var lists = await this.watchListRepository.GetWatchListsForUserAsync(userId, searchParams);
            var paginatedResponse = CursorPaginatedResponse<WatchListDto>.CreateFrom(lists, this.mapper.Map<IEnumerable<WatchListDto>>, searchParams);

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetWatchListForUserAsync")]
        public async Task<ActionResult<WatchListDto>> GetWatchListForUserAsync([FromRoute] int id)
        {
            var list = await this.watchListRepository.GetByIdAsync(id);

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

        [HttpPost]
        public async Task<ActionResult<WatchListDto>> CreateWatchListForUserAsync([FromRoute] int userId, [FromBody] CreateWatchListForUserRequest request)
        {
            var newWatchList = this.mapper.Map<WatchList>(request);
            newWatchList.UserId = userId;

            this.watchListRepository.Add(newWatchList);

            var saveResult = await this.watchListRepository.SaveAllAsync();

            if (!saveResult)
            {
                return this.BadRequest("Unable to create watch list.");
            }

            var mapped = this.mapper.Map<WatchListDto>(newWatchList);

            return this.CreatedAtRoute("GetWatchListForUserAsync", new { id = mapped.Id, userId }, mapped);
        }

        [HttpPost("{id}/items")]
        public async Task<ActionResult<WatchListDto>> AddItemToWatchListForUserAsync([FromRoute] int id, [FromBody] AddItemToWatchListRequest request)
        {
            if (request.Id == null)
            {
                return this.BadRequest($"Id is required.");
            }

            var watchList = await this.watchListRepository.GetByIdAsync(id);

            if (watchList == null)
            {
                return this.NotFound($"No watch list with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(watchList))
            {
                return this.Forbidden("You are not authorized to change this resource.");
            }

            var itemToAdd = await this.itemRepository.GetByIdAsync(request.Id.Value);

            if (watchList == null)
            {
                return this.NotFound($"No item with Id {request.Id.Value} found.");
            }

            watchList.WatchedItems.Add(itemToAdd);

            var saveResults = await this.watchListRepository.SaveAllAsync();

            if (!saveResults)
            {
                return this.BadRequest("Failed to delete the resource.");
            }

            var mapped = this.mapper.Map<WatchListDto>(watchList);

            return this.Ok(mapped);
        }

        [HttpDelete("{id}")]
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

            this.watchListRepository.Delete(watchList);
            var saveResults = await this.watchListRepository.SaveAllAsync();

            return saveResults ? this.NoContent() : this.BadRequest("Failed to delete the resource.");
        }

        [HttpDelete("{id}/items/{itemId}")]
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
    }
}