using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses;
using WoWMarketWatcher.Common.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using WoWMarketWatcher.Common.Constants;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
    [ApiController]
    public class WatchListsController : ServiceControllerBase
    {
        private readonly WatchListRepository watchListRepository;
        private readonly IMapper mapper;


        public WatchListsController(WatchListRepository watchListRepository, IMapper mapper)
        {
            this.watchListRepository = watchListRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<WatchListDto>>> GetWatchListsAsync([FromQuery] RealmQueryParameters searchParams)
        {
            var lists = await this.watchListRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<WatchListDto>.CreateFrom(lists, this.mapper.Map<IEnumerable<WatchListDto>>, searchParams);

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
    }
}