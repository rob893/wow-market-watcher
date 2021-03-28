using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record AddItemToWatchListRequest
    {
        [Required]
        public int? Id { get; init; }
    }
}