using System.Threading.Tasks;

namespace WoWMarketWatcher.API.Services.Events
{
    public interface IEventGridEventSender
    {
        Task SendEventAsync(EventType eventType, object data, string? eventId = null);
    }
}