using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.DTOs
{
    public record WatchListDto : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public string Name { get; init; } = default!;

        public string? Description { get; init; }

        public List<WatchedItemDto> WatchedItems { get; init; } = new();
    }
}