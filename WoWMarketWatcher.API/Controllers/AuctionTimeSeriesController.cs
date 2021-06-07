using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;

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
            var timeSeriesEntries = await this.timeSeriesRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<AuctionTimeSeriesEntryDto, long>>(timeSeriesEntries.ToCursorPaginatedResponse(entry => entry.Id, searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetAuctionTimeSeriesEntryAsync")]
        public async Task<ActionResult<AuctionTimeSeriesEntryDto>> GetAuctionTimeSeriesEntryAsync([FromRoute] int id)
        {
            var entry = await this.timeSeriesRepository.GetByIdAsync(id);

            if (entry == null)
            {
                return this.NotFound($"Time series entry with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<AuctionTimeSeriesEntryDto>(entry);

            return this.Ok(mapped);
        }
    }
}