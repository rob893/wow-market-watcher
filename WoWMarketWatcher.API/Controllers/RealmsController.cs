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
    [Route("api/wow/[controller]")]
    [ApiController]
    public class RealmsController : ServiceControllerBase
    {
        private readonly RealmRepository realmRepository;
        private readonly IMapper mapper;


        public RealmsController(RealmRepository realmRepository, IMapper mapper)
        {
            this.realmRepository = realmRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<RealmDto>>> GetRealmsAsync([FromQuery] RealmQueryParameters searchParams)
        {
            var realms = await this.realmRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<RealmDto>.CreateFrom(realms, this.mapper.Map<IEnumerable<RealmDto>>, searchParams);

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