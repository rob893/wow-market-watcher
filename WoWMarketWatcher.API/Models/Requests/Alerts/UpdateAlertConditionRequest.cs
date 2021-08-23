using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record UpdateAlertConditionRequest
    {
        public int? ConnectedRealmId { get; init; }

        public int? WoWItemId { get; init; }

        public AlertConditionMetric? Metric { get; init; }

        public AlertConditionOperator? Operator { get; init; }

        public AlertConditionAggregationType? AggregationType { get; init; }

        public int? AggregationTimeGranularityInHours { get; init; }

        public long? Threshold { get; init; }
    }
}