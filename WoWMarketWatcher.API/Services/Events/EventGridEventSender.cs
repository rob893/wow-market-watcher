using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Events;
using WoWMarketWatcher.API.Models.Settings;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Services.Events
{
    public sealed class EventGridEventSender : IEventGridEventSender
    {
        private readonly IEventGridPublisherClientFactory clientFactory;

        private readonly ICorrelationIdService correlationIdService;

        private readonly EventGridSettings settings;

        private readonly ILogger<EventGridEventSender> logger;

        public EventGridEventSender(
            IEventGridPublisherClientFactory clientFactory,
            ICorrelationIdService correlationIdService,
            IOptions<EventGridSettings> eventGridSettings,
            ILogger<EventGridEventSender> logger)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.settings = eventGridSettings?.Value ?? throw new ArgumentNullException(nameof(eventGridSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public Task SendConnectedRealmAuctionDataUpdateCompleteEventAsync(int connectedRealmId)
        {
            var eventGridEvent = new EventGridEvent(
                "WMW",
                EventTypes.ConnectedRealmAuctionDataUpdateComplete,
                "1",
                new ConnectedRealmAuctionDataUpdateCompleteEvent
                {
                    ConnectedRealmId = connectedRealmId
                })
            {
                Id = $"{this.CorrelationId}-{connectedRealmId}"
            };

            return this.SendAsync(eventGridEvent);
        }

        public Task SendEventAsync(string eventType, object data)
        {
            var eventGridEvent = new EventGridEvent(
                "WMW",
                eventType,
                "1",
                data)
            {
                Id = this.CorrelationId
            };

            return this.SendAsync(eventGridEvent);
        }

        private async Task SendAsync(EventGridEvent evt)
        {
            var sourceName = GetSourceName();

            if (!this.settings.SendingEnabled)
            {
                this.logger.LogDebug(sourceName, this.CorrelationId, "Event grid publishing disabled.");
                return;
            }

            try
            {
                this.logger.LogDebug(sourceName, this.CorrelationId, $"Sending event to event grid. {nameof(evt.Id)}={evt.Id} {nameof(evt.EventType)}={evt.EventType}.");

                var client = this.clientFactory.CreateClient();

                await client.SendEventAsync(evt);

                this.logger.LogInformation(sourceName, this.CorrelationId, $"Event sent to event grid. {nameof(evt.Id)}={evt.Id} {nameof(evt.EventType)}={evt.EventType}.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(sourceName, this.CorrelationId, $"Failed to send event to event grid ({ex.Message}). {nameof(evt.Id)}={evt.Id} {nameof(evt.EventType)}={evt.EventType}. {ex}");
            }
        }
    }
}