using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record AddItemToWatchListRequest
    {
        [Required]
        public int? Id { get; init; }
    }
}