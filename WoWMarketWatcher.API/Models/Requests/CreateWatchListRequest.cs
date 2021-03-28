using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record CreateWatchListRequest
    {
        [Required]
        public int? UserId { get; init; }
        [Required]
        public int? ConnectedRealmId { get; init; }
        [Required]
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
    }
}