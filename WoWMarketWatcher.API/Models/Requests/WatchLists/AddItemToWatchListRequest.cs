using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.WatchLists
{
    public record AddItemToWatchListRequest
    {
        [Required]
        public int? WoWItemId { get; init; }

        [Required]
        public int? ConnectedRealmId { get; init; }
    }
}