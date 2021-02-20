using System;
using System.Linq;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public class AuctionTimeSeriesRepository : Repository<AuctionTimeSeriesEntry, long, AuctionTimeSeriesQueryParameters>, IAuctionTimeSeriesRepository
    {
        public AuctionTimeSeriesRepository(DataContext context) : base(
            context,
            Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
            str =>
            {
                try
                {
                    return BitConverter.ToInt64(Convert.FromBase64String(str), 0);
                }
                catch
                {
                    throw new ArgumentException($"{str} is not a valid base 64 encoded int64.");
                }
            },
            (source, afterId) => source.Where(item => item.Id > afterId),
            (source, beforeId) => source.Where(item => item.Id < beforeId)
        )
        { }

        protected override IQueryable<AuctionTimeSeriesEntry> AddWhereClauses(IQueryable<AuctionTimeSeriesEntry> query, AuctionTimeSeriesQueryParameters searchParams)
        {
            if (searchParams.StartDate == null || searchParams.EndDate == null)
            {
                throw new ArgumentException($"{nameof(searchParams.StartDate)} and {nameof(searchParams.EndDate)} must not be null.");
            }

            query = query.Where(entry => entry.Timestamp >= searchParams.StartDate.Value && entry.Timestamp <= searchParams.EndDate.Value);

            if (searchParams.ConnectedRealmId != null)
            {
                query = query.Where(entry => entry.ConnectedRealmId == searchParams.ConnectedRealmId.Value);
            }

            if (searchParams.WoWItemId != null)
            {
                query = query.Where(entry => entry.WoWItemId == searchParams.WoWItemId.Value);
            }

            return query;
        }
    }
}