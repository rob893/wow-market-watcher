using System;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.QueryParameters
{
    public record AuctionTimeSeriesQueryParameters : CursorPaginationQueryParameters
    {
        public int? WoWItemId { get; init; }
        public int? ConnectedRealmId { get; init; }
        [Required]
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
    }
}