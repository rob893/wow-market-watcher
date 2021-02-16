using System.Collections.Generic;

namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record ConnectedRealmDto : IIdentifiable<int>
    {
        public int Id { get; init; }
        public List<RealmDto> Realms { get; init; } = new List<RealmDto>();
    }
}