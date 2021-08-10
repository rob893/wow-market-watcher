using System;
using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.Services.Events
{
    public sealed class EventGridPublisherClientFactory : IEventGridPublisherClientFactory
    {
        private readonly EventGridSettings settings;

        private EventGridPublisherClient? eventGridClient;

        public EventGridPublisherClientFactory(IOptions<EventGridSettings> eventGridSettings)
        {
            this.settings = eventGridSettings?.Value ?? throw new ArgumentNullException(nameof(eventGridSettings));
        }

        public EventGridPublisherClient CreateClient()
        {
            if (this.eventGridClient != null)
            {
                return this.eventGridClient;
            }

            this.eventGridClient = new EventGridPublisherClient(this.settings.TopicUrl, new AzureKeyCredential(this.settings.AccessKey));

            return this.eventGridClient;
        }
    }
}