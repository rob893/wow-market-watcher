using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class ConnectedRealm : IIdentifiable<int>
    {
        public int Id { get; init; }

        [MaxLength(50)]
        public string Population { get; set; } = default!;

        public List<Realm> Realms { get; init; } = new();
    }
}