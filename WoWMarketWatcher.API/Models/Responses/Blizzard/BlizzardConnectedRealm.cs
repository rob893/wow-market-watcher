using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardConnectedRealm
    {
        public int Id { get; init; }
        public List<BlizzardLocaleRealm> Realms { get; init; } = new List<BlizzardLocaleRealm>();
    }
}