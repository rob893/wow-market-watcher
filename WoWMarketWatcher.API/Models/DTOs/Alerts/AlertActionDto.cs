using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.DTOs.Alerts
{
    public record AlertActionDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public AlertActionType Type { get; set; }

        public string Target { get; set; } = default!;
    }
}