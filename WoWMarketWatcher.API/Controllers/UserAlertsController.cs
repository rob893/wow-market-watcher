using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Requests;
using WoWMarketWatcher.API.Models.Responses.Pagination;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/users/{userId}/alerts")]
    [ApiController]
    public class UserAlertsController : ServiceControllerBase
    {
        private readonly IAlertRepository alertRepository;
        private readonly IWoWItemRepository itemRepository;
        private readonly IConnectedRealmRepository realmRepository;
        private readonly IMapper mapper;


        public UserAlertsController(IAlertRepository alertRepository, IWoWItemRepository itemRepository, IConnectedRealmRepository realmRepository, IMapper mapper)
        {
            this.alertRepository = alertRepository;
            this.itemRepository = itemRepository;
            this.realmRepository = realmRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<AlertDto>>> GetAlertsForUserAsync([FromRoute] int userId, [FromQuery] CursorPaginationQueryParameters searchParams)
        {
            if (!this.IsUserAuthorizedForResource(userId))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var alerts = await this.alertRepository.GetAlertsForUserAsync(userId, searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<AlertDto>>(alerts.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = nameof(GetAlertUserAsync))]
        public async Task<ActionResult<AlertDto>> GetAlertUserAsync([FromRoute] int id)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"Watch list with id {id} does not exist.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPost]
        public async Task<ActionResult<AlertDto>> CreateAlertForUserAsync([FromRoute] int userId, [FromBody] CreateAlertRequest request)
        {
            var newAlert = this.mapper.Map<Alert>(request);
            newAlert.UserId = userId;

            var realm = await this.realmRepository.GetByIdAsync(newAlert.ConnectedRealmId);

            if (realm == null)
            {
                return this.BadRequest($"No connected realm with id ${newAlert.ConnectedRealmId} exists.");
            }

            var item = await this.itemRepository.GetByIdAsync(newAlert.WoWItemId);

            if (item == null)
            {
                return this.BadRequest($"No item with id ${newAlert.WoWItemId} exists.");
            }

            this.alertRepository.Add(newAlert);

            var saveResult = await this.alertRepository.SaveAllAsync();

            if (!saveResult)
            {
                return this.BadRequest("Unable to alert.");
            }

            var mapped = this.mapper.Map<AlertDto>(newAlert);

            return this.CreatedAtRoute(nameof(GetAlertUserAsync), new { id = mapped.Id, userId }, mapped);
        }
    }
}