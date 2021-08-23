using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class Alert : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        [MaxLength(255)]
        public string Name { get; set; } = default!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        [MaxLength(30)]
        public AlertState State { get; set; }

        public List<AlertCondition> Conditions { get; init; } = new();

        public List<AlertAction> Actions { get; init; } = new();

        public DateTime LastEvaluated { get; set; }

        public DateTime? LastFired { get; set; }
    }
}