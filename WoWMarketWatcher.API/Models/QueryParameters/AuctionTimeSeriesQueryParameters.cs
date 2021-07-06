using System;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.QueryParameters
{
    public record AuctionTimeSeriesQueryParameters : CursorPaginationQueryParameters
    {
        [Required]
        public int? WoWItemId { get; init; }
        [Required]
        public int? ConnectedRealmId { get; init; }
        [Required]
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }
}