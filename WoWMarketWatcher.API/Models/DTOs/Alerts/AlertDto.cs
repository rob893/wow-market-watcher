using System;
using System.Collections.Generic;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.DTOs.Alerts
{
    public record AlertDto : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public string Name { get; init; } = default!;

        public string? Description { get; init; }

        public List<AlertConditionDto> Conditions { get; init; } = new();

        public List<AlertActionDto> Actions { get; init; } = new();

        public AlertState State { get; init; }

        public DateTime LastEvaluated { get; init; }

        public DateTime? LastFired { get; init; }
    }
}