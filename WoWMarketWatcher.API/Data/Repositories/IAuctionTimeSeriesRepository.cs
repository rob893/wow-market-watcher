using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IAuctionTimeSeriesRepository : IRepository<AuctionTimeSeriesEntry, long, AuctionTimeSeriesQueryParameters> { }
}