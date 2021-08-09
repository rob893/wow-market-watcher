using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/wow/items")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class WoWItemsController : ServiceControllerBase
    {
        private readonly IWoWItemRepository itemRepository;

        private readonly IMapper mapper;

        private readonly IMemoryCache cache;

        public WoWItemsController(IWoWItemRepository itemRepository, IMapper mapper, IMemoryCache cache, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Gets a paginated list of items matching the seach critera.
        /// </summary>
        /// <param name="searchParams">The search parameters.</param>
        /// <returns>A paginated list of items matching the seach critera.</returns>
        /// <response code="200">A paginated list of items.</response>
        /// <response code="400">If search parameters are invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet(Name = nameof(GetWoWItemsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<CursorPaginatedResponse<WoWItemDto>>> GetWoWItemsAsync([FromQuery] WoWItemQueryParameters searchParams)
        {
            var items = await this.itemRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<WoWItemDto>>(items.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        /// <summary>
        /// Gets a single item by id.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>A single item if found.</returns>
        /// <response code="200">The item.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("{id}", Name = nameof(GetWoWItemAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Gets a list of item classes.
        /// </summary>
        /// <returns>The list of item classes.</returns>
        /// <response code="200">A list of item classes.</response>
        /// <response code="400">If request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("classes", Name = nameof(GetWoWItemClassesAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        /// <summary>
        /// Gets a list of item subclasses.
        /// </summary>
        /// <returns>The list of item subclasses.</returns>
        /// <response code="200">A list of item subclasses.</response>
        /// <response code="400">If request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("subclasses", Name = nameof(GetWoWItemSubclassesAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        /// <summary>
        /// Gets a list of item qualities.
        /// </summary>
        /// <returns>The list of item qualities.</returns>
        /// <response code="200">A list of item qualities.</response>
        /// <response code="400">If request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("qualities", Name = nameof(GetWoWItemQualitiesAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

        /// <summary>
        /// Gets a list of item inventory types.
        /// </summary>
        /// <returns>The list of item inventory types.</returns>
        /// <response code="200">A list of item inventory types.</response>
        /// <response code="400">If request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpGet("inventoryTypes", Name = nameof(GetWoWItemInventoryTypesAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
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