using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.WatchLists
{
    public record CreateWatchListRequest
    {
        [Required]
        public int? UserId { get; init; }

        [Required]
        [MaxLength(255)]
        public string Name { get; init; } = default!;

        [MaxLength(4000)]
        public string? Description { get; init; }
    }
}