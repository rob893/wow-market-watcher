using Azure.Messaging.EventGrid;

namespace WoWMarketWatcher.API.Services.Events
{
    public interface IEventGridPublisherClientFactory
    {
        EventGridPublisherClient CreateClient();
    }
}