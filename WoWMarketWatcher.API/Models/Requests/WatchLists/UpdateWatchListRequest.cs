using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.WatchLists
{
    public class UpdateWatchListRequest
    {
        [MaxLength(255)]
        public string? Name { get; init; }

        [MaxLength(4000)]
        public string? Description { get; init; }
    }
}