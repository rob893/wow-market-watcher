using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.DTOs.Alerts
{
    public record AlertConditionDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public AlertConditionMetric Metric { get; init; }

        public AlertConditionOperator Operator { get; init; }

        public AlertConditionAggregationType AggregationType { get; init; }

        public int AggregationTimeGranularityInHours { get; init; }

        public int Threshold { get; init; }
    }
}