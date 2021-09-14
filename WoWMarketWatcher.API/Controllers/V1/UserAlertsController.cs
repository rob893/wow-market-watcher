using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs.Alerts;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Requests.Alerts;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/users/{userId}/alerts")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class UserAlertsController : ServiceControllerBase
    {
        private readonly IAlertRepository alertRepository;

        private readonly IWoWItemRepository itemRepository;

        private readonly IConnectedRealmRepository realmRepository;

        private readonly IMapper mapper;

        public UserAlertsController(
            IAlertRepository alertRepository,
            IWoWItemRepository itemRepository,
            IConnectedRealmRepository realmRepository,
            IMapper mapper,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            this.itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            this.realmRepository = realmRepository ?? throw new ArgumentNullException(nameof(realmRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<AlertDto>>> GetAlertsForUserAsync([FromRoute] int userId, [FromQuery] CursorPaginationQueryParameters searchParams)
        {
            if (!this.IsUserAuthorizedForResource(userId))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            var alerts = await this.alertRepository.GetAlertsForUserAsync(userId, searchParams, false);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<AlertDto>>(alerts.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = nameof(GetAlertUserAsync))]
        public async Task<ActionResult<AlertDto>> GetAlertUserAsync([FromRoute] int id)
        {
            var alert = await this.alertRepository.GetByIdAsync(id, false);

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
        public async Task<ActionResult<AlertDto>> CreateAlertForUserAsync([FromRoute] int userId, [FromBody] CreateOrReplaceAlertForUserRequest request)
        {
            if (request == null)
            {
                return this.BadRequest("Bad request body.");
            }

            if (!this.IsUserAuthorizedForResource(userId))
            {
                return this.Forbidden("You are not authorized to access this resource.");
            }

            if (!this.User.TryGetEmailVerified(out var emailVerified) || !emailVerified.Value)
            {
                return this.BadRequest("Your email must be verified before you can create alerts.");
            }

            foreach (var action in request.Actions)
            {
                if (action.Type == AlertActionType.Email && (!this.User.TryGetUserEmail(out var email) || !email.Equals(action.Target, StringComparison.OrdinalIgnoreCase)))
                {
                    return this.BadRequest("You can only create alerts using your verified email.");
                }
            }

            var newAlert = this.mapper.Map<Alert>(request);
            newAlert.UserId = userId;

            var requestedRealmIds = request.Conditions
                .Select(condition => condition.ConnectedRealmId ?? default)
                .ToHashSet();
            var realms = (await this.realmRepository.SearchAsync(realm => requestedRealmIds.Contains(realm.Id), false)).Select(realm => realm.Id).ToHashSet();
            var invalidRealms = requestedRealmIds.Except(realms);

            if (invalidRealms.Any())
            {
                return this.BadRequest(invalidRealms.Select(id => $"No connected realm with id {id} exists."));
            }

            var requestedItemIds = request.Conditions
                .Select(condition => condition.WoWItemId ?? default)
                .ToHashSet();
            var items = (await this.itemRepository.SearchAsync(item => requestedItemIds.Contains(item.Id), false)).Select(item => item.Id).ToHashSet();
            var invalidItems = requestedItemIds.Except(items);

            if (invalidItems.Any())
            {
                return this.BadRequest(invalidItems.Select(id => $"No item with id {id} exists."));
            }

            this.alertRepository.Add(newAlert);

            var saveResult = await this.alertRepository.SaveChangesAsync();

            if (saveResult == 0)
            {
                return this.BadRequest("Unable to create alert.");
            }

            var mapped = this.mapper.Map<AlertDto>(newAlert);

            return this.CreatedAtRoute(nameof(GetAlertUserAsync), new { id = mapped.Id, userId }, mapped);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlertForUserAsync([FromRoute] int id)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to delete this resource.");
            }

            this.alertRepository.Remove(alert);
            var saveResults = await this.alertRepository.SaveChangesAsync();

            return saveResults > 0 ? this.NoContent() : this.BadRequest("Failed to delete the resource.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AlertDto>> ReplaceAlertForUserAsync([FromRoute] int id, [FromBody] CreateOrReplaceAlertForUserRequest request)
        {
            if (request == null)
            {
                return this.BadRequest("Request body cannot be empty.");
            }

            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to update this resource.");
            }

            if (!this.User.TryGetEmailVerified(out var emailVerified) || !emailVerified.Value)
            {
                return this.BadRequest("Your email must be verified before you can create alerts.");
            }

            foreach (var action in request.Actions)
            {
                if (action.Type == AlertActionType.Email && (!this.User.TryGetUserEmail(out var email) || !email.Equals(action.Target, StringComparison.OrdinalIgnoreCase)))
                {
                    return this.BadRequest("You can only create alerts using your verified email.");
                }
            }

            var requestedRealmIds = request.Conditions
                .Select(condition => condition.ConnectedRealmId ?? default)
                .ToHashSet();
            var realms = (await this.realmRepository.SearchAsync(realm => requestedRealmIds.Contains(realm.Id), false)).Select(realm => realm.Id).ToHashSet();
            var invalidRealms = requestedRealmIds.Except(realms);

            if (invalidRealms.Any())
            {
                return this.BadRequest(invalidRealms.Select(id => $"No connected realm with id {id} exists."));
            }

            var requestedItemIds = request.Conditions
                .Select(condition => condition.WoWItemId ?? default)
                .ToHashSet();
            var items = (await this.itemRepository.SearchAsync(item => requestedItemIds.Contains(item.Id), false)).Select(item => item.Id).ToHashSet();
            var invalidItems = requestedItemIds.Except(items);

            if (invalidItems.Any())
            {
                return this.BadRequest(invalidItems.Select(id => $"No item with id {id} exists."));
            }

            this.mapper.Map(request, alert);

            await this.alertRepository.SaveChangesAsync();

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<AlertDto>> UpdateAlertAsync([FromRoute] int id, [FromBody] JsonPatchDocument<UpdateAlertRequest> requestPatchDoc)
        {
            if (requestPatchDoc == null || requestPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to update this resource.");
            }

            if (!requestPatchDoc.IsValid(out var errors))
            {
                return this.BadRequest(errors);
            }

            var patchDoc = this.mapper.Map<JsonPatchDocument<Alert>>(requestPatchDoc);

            patchDoc.ApplyTo(alert);

            await this.alertRepository.SaveChangesAsync();

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPost("{id}/actions")]
        public async Task<ActionResult<AlertDto>> AddActionToAlertForUserAsync([FromRoute] int id, [FromBody] CreateAlertActionRequest request)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to change this resource.");
            }

            var newAction = this.mapper.Map<AlertAction>(request);

            alert.Actions.Add(newAction);

            var saveResults = await this.alertRepository.SaveChangesAsync();

            if (saveResults == 0)
            {
                return this.BadRequest("Failed to add the alert action.");
            }

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpDelete("{id}/actions/{actionId}")]
        public async Task<ActionResult<AlertDto>> RemoveActionFromAlertForUserAsync([FromRoute] int id, [FromRoute] int actionId)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to delete this resource.");
            }

            var actionToRemove = alert.Actions.FirstOrDefault(item => item.Id == actionId);

            if (actionToRemove == null)
            {
                return this.BadRequest($"Alert {id} does not have action {actionId}.");
            }

            alert.Actions.Remove(actionToRemove);

            var saveResults = await this.alertRepository.SaveChangesAsync();

            if (saveResults == 0)
            {
                return this.BadRequest("Failed to delete the resource.");
            }

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPatch("{id}/actions/{actionId}")]
        public async Task<ActionResult<AlertDto>> UpdateAlertActionForUserAsync([FromRoute] int id, [FromRoute] int actionId, [FromBody] JsonPatchDocument<UpdateAlertActionRequest> requestPatchDoc)
        {
            if (requestPatchDoc == null || requestPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to update this resource.");
            }

            if (!requestPatchDoc.IsValid(out var errors))
            {
                return this.BadRequest(errors);
            }

            var action = alert.Actions.FirstOrDefault(condition => condition.Id == actionId);

            if (action == null)
            {
                return this.NotFound($"Alert {id} does not have action {actionId}.");
            }

            var patchDoc = this.mapper.Map<JsonPatchDocument<AlertAction>>(requestPatchDoc);

            patchDoc.ApplyTo(action);

            await this.alertRepository.SaveChangesAsync();

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPut("{id}/actions/{actionId}")]
        public async Task<ActionResult<AlertDto>> ReplaceAlertActionForUserAsync([FromRoute] int id, [FromRoute] int actionId, [FromBody] PutAlertActionRequest request)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to update this resource.");
            }

            var action = alert.Actions.FirstOrDefault(condition => condition.Id == actionId);

            if (action == null)
            {
                return this.NotFound($"Alert {id} does not have action {actionId}.");
            }

            this.mapper.Map(request, action);

            await this.alertRepository.SaveChangesAsync();

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPost("{id}/conditions")]
        public async Task<ActionResult<AlertDto>> AddConditionToAlertForUserAsync([FromRoute] int id, [FromBody] CreateAlertConditionRequest request)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to change this resource.");
            }

            var newCondition = this.mapper.Map<AlertCondition>(request);

            alert.Conditions.Add(newCondition);

            var saveResults = await this.alertRepository.SaveChangesAsync();

            if (saveResults == 0)
            {
                return this.BadRequest("Failed to add the alert condition.");
            }

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpDelete("{id}/conditions/{conditionId}")]
        public async Task<ActionResult<AlertDto>> RemoveConditionFromAlertForUserAsync([FromRoute] int id, [FromRoute] int conditionId)
        {
            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to delete this resource.");
            }

            var conditionToRemove = alert.Conditions.FirstOrDefault(item => item.Id == conditionId);

            if (conditionToRemove == null)
            {
                return this.BadRequest($"Alert {id} does not have condition {conditionId}.");
            }

            alert.Conditions.Remove(conditionToRemove);

            var saveResults = await this.alertRepository.SaveChangesAsync();

            if (saveResults == 0)
            {
                return this.BadRequest("Failed to delete the resource.");
            }

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }

        [HttpPatch("{id}/conditions/{conditionId}")]
        public async Task<ActionResult<AlertDto>> UpdateAlertConditionForUserAsync([FromRoute] int id, [FromRoute] int conditionId, [FromBody] JsonPatchDocument<UpdateAlertConditionRequest> requestPatchDoc)
        {
            if (requestPatchDoc == null || requestPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            var alert = await this.alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                return this.NotFound($"No resource with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(alert))
            {
                return this.Forbidden("You are not authorized to update this resource.");
            }

            if (!requestPatchDoc.IsValid(out var errors))
            {
                return this.BadRequest(errors);
            }

            var condition = alert.Conditions.FirstOrDefault(condition => condition.Id == conditionId);

            if (condition == null)
            {
                return this.NotFound($"Alert {id} does not have condition {conditionId}.");
            }

            var patchDoc = this.mapper.Map<JsonPatchDocument<AlertCondition>>(requestPatchDoc);

            patchDoc.ApplyTo(condition);

            await this.alertRepository.SaveChangesAsync();

            var mapped = this.mapper.Map<AlertDto>(alert);

            return this.Ok(mapped);
        }
    }
}