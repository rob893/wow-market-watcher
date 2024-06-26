using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record CreateAlertConditionRequest
    {
        [Required]
        public int? ConnectedRealmId { get; init; }

        [Required]
        public int? WoWItemId { get; init; }

        [Required]
        public AlertConditionMetric? Metric { get; init; }

        [Required]
        public AlertConditionOperator? Operator { get; init; }

        [Required]
        public AlertConditionAggregationType? AggregationType { get; init; }

        [Required]
        public int? AggregationTimeGranularityInHours { get; init; }

        [Required]
        public long? Threshold { get; init; }
    }
}