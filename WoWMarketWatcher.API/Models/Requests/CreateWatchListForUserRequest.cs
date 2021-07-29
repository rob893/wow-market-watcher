using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record CreateWatchListForUserRequest
    {
        [Required]
        public int? ConnectedRealmId { get; init; }
        [Required]
        [MaxLength(255)]
        public string Name { get; init; } = default!;
        [MaxLength(4000)]
        public string? Description { get; init; }
    }
}