using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Entities;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record CreateAlertRequest
    {
        [Required]
        public int? UserId { get; init; }

        [Required]
        public int? ConnectedRealmId { get; init; }

        [Required]
        public int? WoWItemId { get; init; }

        [Required]
        [MaxLength(255)]
        public string Name { get; init; } = default!;

        [MaxLength(4000)]
        public string? Description { get; init; }

        [Required]
        public List<CreateAlertActionRequest> Actions { get; init; } = new();

        [Required]
        public List<CreateAlertConditionRequest> Conditions { get; init; } = new();
    }

    public record CreateAlertActionRequest
    {
        [Required]
        public AlertActionType Type { get; init; }

        [Required]
        [MaxLength(50)]
        public string Target { get; init; } = default!;
    }

    public record CreateAlertConditionRequest
    {
        [Required]
        public AlertConditionMetric Metric { get; init; }

        [Required]
        public AlertConditionOperator Operator { get; init; }

        [Required]
        public AlertConditionAggregationType AggregationType { get; init; }

        [Required]
        public int? AggregationTimeGranularityInHours { get; init; }

        [Required]
        public int? Threshold { get; init; }
    }
}