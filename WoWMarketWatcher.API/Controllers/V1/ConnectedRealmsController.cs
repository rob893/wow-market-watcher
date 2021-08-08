using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs.Realms;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/wow/[controller]")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class ConnectedRealmsController : ServiceControllerBase
    {
        private readonly IConnectedRealmRepository connectedRealmRepository;

        private readonly IMapper mapper;

        public ConnectedRealmsController(IConnectedRealmRepository connectedRealmRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.connectedRealmRepository = connectedRealmRepository ?? throw new ArgumentNullException(nameof(connectedRealmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<ConnectedRealmDto>>> GetConnectedRealmsAsync([FromQuery] CursorPaginationQueryParameters searchParams)
        {
            var realms = await this.connectedRealmRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<ConnectedRealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

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
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<RealmDto>>(realms.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }
    }
}