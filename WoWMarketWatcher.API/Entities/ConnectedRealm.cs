using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class ConnectedRealm : IIdentifiable<int>
    {
        public int Id { get; init; }

        [MaxLength(50)]
        public string Population { get; set; } = default!;

        public List<Realm> Realms { get; init; } = new List<Realm>();
    }
}