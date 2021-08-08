using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class WoWItemRepository : Repository<WoWItem, WoWItemQueryParameters>, IWoWItemRepository
    {
        public WoWItemRepository(DataContext context) : base(context) { }

        public Task<List<string>> GetItemQualitiesAsync()
        {
            return this.Context.WoWItems.Select(item => item.Quality).Distinct().ToListAsync();
        }

        public Task<List<string>> GetItemClassesAsync()
        {
            return this.Context.WoWItems.Select(item => item.ItemClass).Distinct().ToListAsync();
        }

        public Task<List<string>> GetItemSubclassesAsync()
        {
            return this.Context.WoWItems.Select(item => item.ItemSubclass).Distinct().ToListAsync();
        }

        public Task<List<string>> GetItemInventoryTypesAsync()
        {
            return this.Context.WoWItems.Select(item => item.InventoryType).Distinct().ToListAsync();
        }

        protected override IQueryable<WoWItem> AddWhereClauses(IQueryable<WoWItem> query, WoWItemQueryParameters searchParams)
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            // MySQL does ignore string case by default. Forcing ignore case here adds large performance overhead
            if (searchParams.InventoryType != null)
            {
                query = query.Where(item => item.InventoryType == searchParams.InventoryType);
            }

            if (searchParams.Quality != null)
            {
                query = query.Where(item => item.Quality == searchParams.Quality);
            }

            if (searchParams.ItemClass != null)
            {
                query = query.Where(item => item.ItemClass == searchParams.ItemClass);
            }

            if (searchParams.ItemSubclass != null)
            {
                query = query.Where(item => item.ItemSubclass == searchParams.ItemSubclass);
            }

            if (searchParams.Name != null)
            {
                query = query.Where(item => item.Name == searchParams.Name);
            }

            if (searchParams.NameLike != null)
            {
                query = query.Where(item => item.Name.Contains(searchParams.NameLike));
            }

            return query;
        }
    }
}