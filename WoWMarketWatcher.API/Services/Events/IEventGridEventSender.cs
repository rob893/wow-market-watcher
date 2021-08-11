using System.Threading.Tasks;

namespace WoWMarketWatcher.API.Services.Events
{
    public interface IEventGridEventSender
    {
        Task SendConnectedRealmAuctionDataUpdateCompleteEventAsync(int connectedRealmId);

        Task SendEventAsync(string eventType, object data);
    }
}