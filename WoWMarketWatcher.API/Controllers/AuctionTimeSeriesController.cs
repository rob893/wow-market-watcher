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
    public class AuctionTimeSeriesController : ServiceControllerBase
    {
        private readonly AuctionTimeSeriesRepository timeSeriesRepository;
        private readonly IMapper mapper;


        public AuctionTimeSeriesController(AuctionTimeSeriesRepository timeSeriesRepository, IMapper mapper)
        {
            this.timeSeriesRepository = timeSeriesRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>> GetAuctionTimeSeriesAsync([FromQuery] AuctionTimeSeriesQueryParameters searchParams)
        {
            var realms = await this.timeSeriesRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>.CreateFrom(realms, this.mapper.Map<IEnumerable<AuctionTimeSeriesEntryDto>>, searchParams);

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