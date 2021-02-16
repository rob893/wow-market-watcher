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

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/users/{userId}/watchLists")]
    [ApiController]
    public class UserWatchListsController : ServiceControllerBase
    {
        private readonly WatchListRepository watchListRepository;
        private readonly IMapper mapper;


        public UserWatchListsController(WatchListRepository watchListRepository, IMapper mapper)
        {
            this.watchListRepository = watchListRepository;
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

            return this.CreatedAtRoute("GetWatchListForUserAsync", (id: mapped.Id, userId), mapped);
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

            return saveResults ? this.NoContent() : this.BadRequest("Failed to delete the income.");
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