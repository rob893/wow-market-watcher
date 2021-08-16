using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public abstract class Repository<TEntity, TEntityKey, TSearchParams> : IRepository<TEntity, TEntityKey, TSearchParams>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        where TSearchParams : CursorPaginationQueryParameters
    {
        protected Repository(DataContext context, Func<TEntityKey, string> convertIdToBase64, Func<string, TEntityKey> convertBase64ToIdType)
        {
            this.Context = context ?? throw new ArgumentNullException(nameof(context));
            this.ConvertIdToBase64 = convertIdToBase64 ?? throw new ArgumentNullException(nameof(convertIdToBase64));
            this.ConvertBase64ToIdType = convertBase64ToIdType ?? throw new ArgumentNullException(nameof(convertBase64ToIdType));
        }

        protected DataContext Context { get; }

        protected Func<TEntityKey, string> ConvertIdToBase64 { get; }

        protected Func<string, TEntityKey> ConvertBase64ToIdType { get; }

        public void Add(TEntity entity)
        {
            this.Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            this.Context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(TEntity entity)
        {
            this.BeforeRemove(entity);
            this.Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                this.BeforeRemove(entity);
            }

            this.Context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.Context.SaveChangesAsync() > 0;
        }

        public Task<int> SaveChangesAsync()
        {
            return this.Context.SaveChangesAsync();
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, bool track = true)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = this.AddIncludes(query);

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(condition);
        }

        public Task<TEntity> GetByIdAsync(TEntityKey id, bool track = true)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = this.AddIncludes(query);

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public Task<TEntity> GetByIdAsync(TEntityKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            query = this.AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public Task<List<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, bool track = true)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = this.AddIncludes(query);

            return query.Where(condition).ToListAsync();
        }

        public Task<CursorPaginatedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, bool track = true)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            if (!track)
            {
                query = query.AsNoTracking();
            }

            query = this.AddIncludes(query);
            query = this.AddWhereClauses(query, searchParams);

            return query.ToCursorPaginatedListAsync(
                item => item.Id,
                this.ConvertIdToBase64,
                this.ConvertBase64ToIdType,
                searchParams);
        }

        protected virtual IQueryable<TEntity> AddWhereClauses(IQueryable<TEntity> query, TSearchParams searchParams)
        {
            return query;
        }

        protected virtual void BeforeRemove(TEntity entity) { }

        protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
        {
            return query;
        }
    }

    public abstract class Repository<TEntity, TSearchParams> : Repository<TEntity, int, TSearchParams>, IRepository<TEntity, int, TSearchParams>
        where TEntity : class, IIdentifiable<int>
        where TSearchParams : CursorPaginationQueryParameters
    {
        protected Repository(DataContext context) : base(
            context,
            Id => Id.ConvertToBase64Url(),
            str =>
            {
                try
                {
                    return str.ConvertToInt32FromBase64Url();
                }
                catch
                {
                    throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
                }
            })
        { }
    }
}