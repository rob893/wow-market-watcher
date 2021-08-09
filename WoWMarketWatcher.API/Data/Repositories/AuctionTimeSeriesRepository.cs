using System;
using System.Linq;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class AuctionTimeSeriesRepository : Repository<AuctionTimeSeriesEntry, long, AuctionTimeSeriesQueryParameters>, IAuctionTimeSeriesRepository
    {
        public AuctionTimeSeriesRepository(DataContext context) : base(
            context,
            Id => Id.ConvertToBase64Url(),
            str =>
            {
                try
                {
                    return str.ConvertToLongFromBase64Url();
                }
                catch
                {
                    throw new ArgumentException($"{str} is not a valid base 64 encoded int64.");
                }
            }
        )
        { }

        protected override IQueryable<AuctionTimeSeriesEntry> AddWhereClauses(IQueryable<AuctionTimeSeriesEntry> query, AuctionTimeSeriesQueryParameters searchParams)
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            if (searchParams.WoWItemId != null)
            {
                query = query.Where(entry => entry.WoWItemId == searchParams.WoWItemId.Value);
            }

            if (searchParams.ConnectedRealmId != null)
            {
                query = query.Where(entry => entry.ConnectedRealmId == searchParams.ConnectedRealmId.Value);
            }

            if (searchParams.StartDate != null)
            {
                query = query.Where(entry => entry.Timestamp >= searchParams.StartDate.Value);
            }

            if (searchParams.EndDate != null)
            {
                query = query.Where(entry => entry.Timestamp <= searchParams.EndDate.Value);
            }

            return query;
        }
    }
}