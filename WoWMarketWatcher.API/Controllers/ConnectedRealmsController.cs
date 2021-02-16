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
    public class ConnectedRealmsController : ServiceControllerBase
    {
        private readonly ConnectedRealmRepository connectedRealmRepository;
        private readonly IMapper mapper;


        public ConnectedRealmsController(ConnectedRealmRepository connectedRealmRepository, IMapper mapper)
        {
            this.connectedRealmRepository = connectedRealmRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<ConnectedRealmDto>>> GetConnectedRealmsAsync([FromQuery] CursorPaginationParameters searchParams)
        {
            var realms = await this.connectedRealmRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<ConnectedRealmDto>.CreateFrom(realms, this.mapper.Map<IEnumerable<ConnectedRealmDto>>, searchParams);

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetConnectedRealmAsync")]
        public async Task<ActionResult<ConnectedRealmDto>> GetConnectedRealmAsync([FromRoute] int id)
        {
            var connectedRealm = await this.connectedRealmRepository.GetByIdAsync(id);

            if (connectedRealm == null)
            {
                return this.NotFound($"Connected realm with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<ConnectedRealmDto>(connectedRealm);

            return this.Ok(mapped);
        }

        [HttpGet("{id}/realms")]
        public async Task<ActionResult<CursorPaginatedResponse<RealmDto>>> GetRealmsForConnectedRealmAsync([FromRoute] int id, [FromQuery] RealmQueryParameters searchParams)
        {
            var connectedRealm = await this.connectedRealmRepository.GetByIdAsync(id);

            if (connectedRealm == null)
            {
                return this.NotFound($"Connected realm with id {id} does not exist.");
            }

            var realms = await this.connectedRealmRepository.GetRealmsForConnectedRealmAsync(connectedRealm.Id, searchParams);
            var paginatedResponse = CursorPaginatedResponse<RealmDto>.CreateFrom(realms, this.mapper.Map<IEnumerable<RealmDto>>, searchParams);

            return this.Ok(paginatedResponse);
        }
    }
}