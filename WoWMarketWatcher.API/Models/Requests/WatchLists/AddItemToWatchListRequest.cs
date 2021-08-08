using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.WatchLists
{
    public record AddItemToWatchListRequest
    {
        [Required]
        public int? Id { get; init; }
    }
}