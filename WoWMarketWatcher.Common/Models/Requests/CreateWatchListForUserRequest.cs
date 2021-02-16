using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record CreateWatchListForUserRequest
    {
        [Required]
        public int? ConnectedRealmId { get; init; }
        [Required]
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
    }
}