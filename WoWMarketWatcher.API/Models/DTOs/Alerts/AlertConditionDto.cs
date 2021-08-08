using WoWMarketWatcher.API.Entities;

namespace WoWMarketWatcher.API.Models.DTOs.Alerts
{
    public record AlertConditionDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public AlertConditionMetric Metric { get; set; }

        public AlertConditionOperator Operator { get; set; }

        public AlertConditionAggregationType AggregationType { get; set; }

        public int AggregationTimeGranularityInHours { get; set; }

        public int Threshold { get; set; }
    }
}