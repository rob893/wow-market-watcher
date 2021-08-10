using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Extensions;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Services.Events
{
    public sealed class EventGridEventSender : IEventGridEventSender
    {
        private readonly IEventGridPublisherClientFactory clientFactory;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<EventGridEventSender> logger;

        public EventGridEventSender(IEventGridPublisherClientFactory clientFactory, ICorrelationIdService correlationIdService, ILogger<EventGridEventSender> logger)
        {
            this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public Task SendEventAsync(EventType eventType, object data, string? eventId = null)
        {
            var eventGridEvent = new EventGridEvent(
                "Some subject",
                $"WMW.{eventType}",
                "1",
                data)
            {
                Id = eventId ?? Guid.NewGuid().ToString()
            };

            return this.SendAsync(eventGridEvent);
        }

        private async Task SendAsync(EventGridEvent evt)
        {
            var sourceName = GetSourceName();

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