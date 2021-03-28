using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/wow/[controller]")]
    [ApiController]
    public class AuctionTimeSeriesController : ServiceControllerBase
    {
        private readonly IAuctionTimeSeriesRepository timeSeriesRepository;
        private readonly IMapper mapper;


        public AuctionTimeSeriesController(IAuctionTimeSeriesRepository timeSeriesRepository, IMapper mapper)
        {
            this.timeSeriesRepository = timeSeriesRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>> GetAuctionTimeSeriesAsync([FromQuery] AuctionTimeSeriesQueryParameters searchParams)
        {
            var realms = await this.timeSeriesRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponseFactory.CreateFrom(realms, this.mapper.Map<IEnumerable<AuctionTimeSeriesEntryDto>>, searchParams);

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetAuctionTimeSeriesEntryAsync")]
        public async Task<ActionResult<AuctionTimeSeriesEntryDto>> GetAuctionTimeSeriesEntryAsync([FromRoute] int id)
        {
            var realm = await this.timeSeriesRepository.GetByIdAsync(id);

            if (realm == null)
            {
                return this.NotFound($"Time series entry with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<AuctionTimeSeriesEntryDto>(realm);

            return this.Ok(mapped);
        }
    }
}