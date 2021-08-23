using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class AlertCondition : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public Alert Alert { get; init; } = default!;

        public int WoWItemId { get; set; }

        public WoWItem WoWItem { get; set; } = default!;

        public int ConnectedRealmId { get; set; }

        public ConnectedRealm ConnectedRealm { get; set; } = default!;

        [MaxLength(30)]
        public AlertConditionMetric Metric { get; set; }

        [MaxLength(30)]
        public AlertConditionOperator Operator { get; set; }

        [MaxLength(30)]
        public AlertConditionAggregationType AggregationType { get; set; }

        public int AggregationTimeGranularityInHours { get; set; }

        public long Threshold { get; set; }
    }
}