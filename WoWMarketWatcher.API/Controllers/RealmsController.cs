using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/wow/[controller]")]
    [ApiController]
    public class RealmsController : ServiceControllerBase
    {
        private readonly IRealmRepository realmRepository;
        private readonly IMapper mapper;


        public RealmsController(IRealmRepository realmRepository, IMapper mapper)
        {
            this.realmRepository = realmRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<RealmDto>>> GetRealmsAsync([FromQuery] RealmQueryParameters searchParams)
        {
            var realms = await this.realmRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<RealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetRealmAsync")]
        public async Task<ActionResult<RealmDto>> GetRealmAsync([FromRoute] int id)
        {
            var realm = await this.realmRepository.GetByIdAsync(id);

            if (realm == null)
            {
                return this.NotFound($"Realm with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<RealmDto>(realm);

            return this.Ok(mapped);
        }
    }
}