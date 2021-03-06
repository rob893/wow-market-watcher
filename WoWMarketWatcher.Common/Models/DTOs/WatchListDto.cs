using System.Collections.Generic;

namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record WatchListDto : IIdentifiable<int>
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int ConnectedRealmId { get; init; }
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public List<WoWItemDto> WatchedItems { get; init; } = new List<WoWItemDto>();
    }
}