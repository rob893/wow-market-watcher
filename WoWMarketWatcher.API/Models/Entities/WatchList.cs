using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class WatchList : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        [MaxLength(255)]
        public string Name { get; set; } = default!;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public List<WatchedItem> WatchedItems { get; set; } = new();
    }
}