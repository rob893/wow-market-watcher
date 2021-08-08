using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record CreateAlertForUserRequest
    {
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
}