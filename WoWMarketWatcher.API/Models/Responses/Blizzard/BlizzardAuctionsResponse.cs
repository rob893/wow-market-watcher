using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Responses.Blizzard
{
    public record BlizzardAuctionsResponse
    {
        public List<BlizzardAuction> Auctions { get; init; } = new List<BlizzardAuction>();
    }
}