using System;
using System.Collections.Generic;
using WoWMarketWatcher.API.Entities;

namespace WoWMarketWatcher.API.Models.DTOs
{
    public record AlertDto : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public int WoWItemId { get; set; }

        public int ConnectedRealmId { get; set; }

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public List<AlertConditionDto> Conditions { get; init; } = new();

        public List<AlertActionDto> Actions { get; init; } = new();

        public DateTime LastEvaluated { get; set; }

        public DateTime? LastFired { get; set; }
    }

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

    public record AlertActionDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public AlertActionType Type { get; set; }

        public string Target { get; set; } = default!;
    }
}