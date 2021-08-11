using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Services;
using WoWMarketWatcher.API.Services.Events;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/eventListeners")]
    [ApiVersion("1")]
    [AllowAnonymous]
    [ApiController]
    public sealed class EventListenersController : ServiceControllerBase
    {
        public EventListenersController(ICorrelationIdService correlationIdService) : base(correlationIdService) { }

        [HttpPost("eventGrid", Name = nameof(ProcessEventGridEventsAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ProcessEventGridEventsAsync([FromBody] EventGridEvent[] events)
        {
            if (events == null)
            {
                return this.BadRequest("No events were found in the request body.");
            }

            foreach (var eventGridEvent in events)
            {
                if (eventGridEvent.TryGetSystemEventData(out var eventData))
                {
                    if (eventData is SubscriptionValidationEventData subscriptionValidationEventData)
                    {
                        var responseData = new SubscriptionValidationResponse
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };

                        return this.Ok(responseData);
                    }
                }
                else
                {
                    switch (eventGridEvent.EventType)
                    {
                        case EventTypes.ConnectedRealmAuctionDataUpdateComplete:
                            break;
                        default:
                            break;
                    }
                    var auctionDataProcessedData = eventGridEvent.Data.ToObjectFromJson<Dictionary<string, string>>();
                    return this.Ok(auctionDataProcessedData);
                }
            }

            return this.NoContent();
        }
    }
}