using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class Alert : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        public int WoWItemId { get; set; }

        public WoWItem WoWItem { get; set; } = default!;

        public int ConnectedRealmId { get; set; }

        public ConnectedRealm ConnectedRealm { get; set; } = default!;

        [MaxLength(255)]
        public string Name { get; set; } = default!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public List<AlertCondition> Conditions { get; init; } = new();

        public List<AlertAction> Actions { get; init; } = new();

        public DateTime LastEvaluated { get; set; }

        public DateTime? LastFired { get; set; }
    }

    public class AlertAction : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public Alert Alert { get; init; } = default!;

        [MaxLength(30)]
        public AlertActionType Type { get; set; }

        [MaxLength(50)]
        public string Target { get; set; } = default!;
    }

    public enum AlertActionType
    {
        Email
    }

    public class AlertCondition : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public Alert Alert { get; init; } = default!;

        [MaxLength(30)]
        public AlertConditionMetric Metric { get; set; }

        [MaxLength(30)]
        public AlertConditionOperator Operator { get; set; }

        [MaxLength(30)]
        public AlertConditionAggregationType AggregationType { get; set; }

        public int AggregationTimeGranularityInHours { get; set; }

        public long Threshold { get; set; }
    }

    public enum AlertConditionMetric
    {
        TotalAvailableForAuction,
        AveragePrice,
        MinPrice,
        MaxPrice,
        Price25Percentile,
        Price50Percentile,
        Price75Percentile,
        Price95Percentile,
        Price99Percentile
    }

    public enum AlertConditionOperator
    {
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo
    }

    public enum AlertConditionAggregationType
    {
        Sum,
        Count,
        Average,
        Min,
        Max
    }
}