using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses;
using Microsoft.Extensions.Caching.Memory;
using WoWMarketWatcher.API.Constants;
using System;
using WoWMarketWatcher.Common.Models.DTOs;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/wow/items")]
    [ApiController]
    public class WoWItemsController : ServiceControllerBase
    {
        private readonly WoWItemRepository itemRepository;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;


        public WoWItemsController(WoWItemRepository itemRepository, IMapper mapper, IMemoryCache cache)
        {
            this.itemRepository = itemRepository;
            this.mapper = mapper;
            this.cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<WoWItemDto>>> GetWoWItemsAsync([FromQuery] WoWItemQueryParameters searchParams)
        {
            var items = await this.itemRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<WoWItemDto>.CreateFrom(items, this.mapper.Map<IEnumerable<WoWItemDto>>, searchParams);

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetWoWItemAsync")]
        public async Task<ActionResult<WoWItemDto>> GetWoWItemAsync([FromRoute] int id)
        {
            var item = await this.itemRepository.GetByIdAsync(id);

            if (item == null)
            {
                return this.NotFound($"Item with id {id} does not exist.");
            }

            var mapped = this.mapper.Map<WoWItemDto>(item);

            return this.Ok(mapped);
        }

        [HttpGet("classes")]
        public async Task<ActionResult<IEnumerable<string>>> GetWoWItemClassesAsync()
        {
            if (this.cache.TryGetValue<IEnumerable<string>>(CacheKeys.WoWItemClassesKey, out var cachedResults))
            {
                return this.Ok(cachedResults);
            }

            var results = await this.itemRepository.GetItemClassesAsync();

            this.cache.Set(CacheKeys.WoWItemClassesKey, results, TimeSpan.FromDays(1));

            return this.Ok(results);
        }

        [HttpGet("subclasses")]
        public async Task<ActionResult<IEnumerable<string>>> GetWoWItemSubclassesAsync()
        {
            if (this.cache.TryGetValue<IEnumerable<string>>(CacheKeys.WoWItemSubclassesKey, out var cachedResults))
            {
                return this.Ok(cachedResults);
            }

            var results = await this.itemRepository.GetItemSubclassesAsync();

            this.cache.Set(CacheKeys.WoWItemSubclassesKey, results, TimeSpan.FromDays(1));

            return this.Ok(results);
        }

        [HttpGet("qualities")]
        public async Task<ActionResult<IEnumerable<string>>> GetWoWItemQualitiesAsync()
        {
            if (this.cache.TryGetValue<IEnumerable<string>>(CacheKeys.WoWItemQualitiesKey, out var cachedResults))
            {
                return this.Ok(cachedResults);
            }

            var results = await this.itemRepository.GetItemQualitiesAsync();

            this.cache.Set(CacheKeys.WoWItemQualitiesKey, results, TimeSpan.FromDays(1));

            return this.Ok(results);
        }

        [HttpGet("inventoryTypes")]
        public async Task<ActionResult<IEnumerable<string>>> GetWoWItemInventoryTypesAsync()
        {
            if (this.cache.TryGetValue<IEnumerable<string>>(CacheKeys.WoWItemInventoryTypesKey, out var cachedResults))
            {
                return this.Ok(cachedResults);
            }

            var results = await this.itemRepository.GetItemInventoryTypesAsync();

            this.cache.Set(CacheKeys.WoWItemInventoryTypesKey, results, TimeSpan.FromDays(1));

            return this.Ok(results);
        }
    }
}