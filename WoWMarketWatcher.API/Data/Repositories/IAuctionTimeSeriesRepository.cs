using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IAuctionTimeSeriesRepository : IRepository<AuctionTimeSeriesEntry, long, AuctionTimeSeriesQueryParameters> { }
}