using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record UpdateAlertConditionRequest
    {
        public AlertConditionMetric? Metric { get; init; }

        public AlertConditionOperator? Operator { get; init; }

        public AlertConditionAggregationType? AggregationType { get; init; }

        public int? AggregationTimeGranularityInHours { get; init; }

        public int? Threshold { get; init; }
    }
}