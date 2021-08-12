using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Events;
using WoWMarketWatcher.API.Services;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/eventListeners")]
    [ApiVersion("1")]
    [AllowAnonymous]
    [ApiController]
    public sealed class EventListenersController : ServiceControllerBase
    {
        private readonly IAlertRepository alertRepository;

        private readonly IAlertService alertService;

        private readonly ILogger<EventListenersController> logger;

        public EventListenersController(
            IAlertRepository alertRepository,
            IAlertService alertService,
            ILogger<EventListenersController> logger,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            this.alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes event grid events.
        /// </summary>
        /// <param name="events">The events from event grid.</param>
        /// <returns>The SubscriptionValidationResponse for SubscriptionValidation events. No content for all others.</returns>
        /// <response code="200">Successful processing of the subscription validation event.</response>
        /// <response code="204">Successful processing of non subscription validation events.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided JWT is invalid (expired, bad signature, etc).</response>
        /// <response code="403">If provided JWT is valid but missing required authorization.</response>
        /// <response code="404">If entry is not found.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("eventGrid", Name = nameof(ProcessEventGridEventsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ProcessEventGridEventsAsync([FromBody] EventGridEvent[] events)
        {
            var sourceName = GetSourceName();

            if (events == null || events.Length == 0)
            {
                this.logger.LogWarning(sourceName, this.CorrelationId, "No events were found in the request body.");
                return this.BadRequest("No events were found in the request body.");
            }

            this.logger.LogDebug(sourceName, this.CorrelationId, $"Processing {events.Length} events from event grid.");

            foreach (var eventGridEvent in events)
            {
                if (eventGridEvent.TryGetSystemEventData(out var eventData))
                {
                    if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                    {
                        this.logger.LogDebug(sourceName, this.CorrelationId, "Subscription validation event received.");

                        var responseData = new SubscriptionValidationResponse
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };

                        this.logger.LogInformation(sourceName, this.CorrelationId, "Subscription validation event processed.");

                        return this.Ok(responseData);
                    }
                }
                else
                {
                    switch (eventGridEvent.EventType)
                    {
                        case EventTypes.ConnectedRealmAuctionDataUpdateComplete:
                            this.logger.LogDebug(sourceName, this.CorrelationId, $"{EventTypes.ConnectedRealmAuctionDataUpdateComplete} event received.");
                            await this.HandleConnectedRealmAuctionDataUpdateCompleteEventAsync(eventGridEvent);
                            this.logger.LogInformation(sourceName, this.CorrelationId, $"{EventTypes.ConnectedRealmAuctionDataUpdateComplete} event processed.");
                            break;
                        default:
                            break;
                    }
                }
            }

            return this.NoContent();
        }

        private async Task HandleConnectedRealmAuctionDataUpdateCompleteEventAsync(EventGridEvent eventGridEvent)
        {
            if (eventGridEvent == null)
            {
                throw new ArgumentNullException(nameof(eventGridEvent));
            }

            if (eventGridEvent.EventType != EventTypes.ConnectedRealmAuctionDataUpdateComplete)
            {
                throw new ArgumentException($"This method only supports {EventTypes.ConnectedRealmAuctionDataUpdateComplete} events.", nameof(eventGridEvent));
            }

            var sourceName = GetSourceName();

            var eventData = eventGridEvent.Data.ToObjectFromJson<ConnectedRealmAuctionDataUpdateCompleteEvent>();

            var alertsToProcess = await this.alertRepository.SearchAsync(alert => alert.ConnectedRealmId == eventData.ConnectedRealmId);

            this.logger.LogDebug(sourceName, this.CorrelationId, $"{alertsToProcess.Count} alerts will be processed for connected realm {eventData.ConnectedRealmId}.");

            var alertProcessingErrors = new List<Exception>();
            var alertsProcessed = 0;

            foreach (var alert in alertsToProcess)
            {
                try
                {
                    await this.alertService.EvaluateAlertAsync(alert);
                    alertsProcessed++;
                }
                catch (Exception e)
                {
                    this.logger.LogError(sourceName, this.CorrelationId, $"Error while processing alert {alert.Id}: {e}");
                    alertProcessingErrors.Add(e);
                }
            }

            if (alertProcessingErrors.Any())
            {
                throw new AggregateException($"{alertProcessingErrors.Count} errors encountered while processing alerts for connected realm {eventData.ConnectedRealmId}.", alertProcessingErrors);
            }
            else
            {
                this.logger.LogInformation(sourceName, this.CorrelationId, $"{alertsProcessed} alerts processed for connected realm {eventData.ConnectedRealmId}.");
            }
        }
    }
}