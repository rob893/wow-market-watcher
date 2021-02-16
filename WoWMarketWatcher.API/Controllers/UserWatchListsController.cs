using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses;
using WoWMarketWatcher.Common.Models.DTOs;

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
    }
}