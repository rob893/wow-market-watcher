using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.DTOs.Realms
{
    public record ConnectedRealmDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public string Population { get; init; } = default!;

        public List<RealmDto> Realms { get; init; } = new();
    }
}