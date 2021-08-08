using System;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.DTOs.Alerts
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
}