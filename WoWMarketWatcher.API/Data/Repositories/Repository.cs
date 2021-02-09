using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public abstract class Repository<TEntity, TEntityKey, RSearchParams>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        where RSearchParams : CursorPaginationParameters
    {
        protected readonly DataContext context;
        protected readonly Func<TEntityKey, string> ConvertIdToBase64;
        protected readonly Func<string, TEntityKey> ConvertBase64ToIdType;
        protected readonly Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddAfterExp;
        protected readonly Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddBeforeExp;

        public Repository(DataContext context, Func<TEntityKey, string> ConvertIdToBase64, Func<string, TEntityKey> ConvertBase64ToIdType,
            Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddAfterExp, Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddBeforeExp)
        {
            this.context = context;
            this.ConvertIdToBase64 = ConvertIdToBase64;
            this.ConvertBase64ToIdType = ConvertBase64ToIdType;
            this.AddAfterExp = AddAfterExp;
            this.AddBeforeExp = AddBeforeExp;
        }

        public EntityEntry<TEntity> Entry(TEntity entity)
        {
            return context.Entry(entity);
        }

        public void Add(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().AddRange(entities);
        }

        public void Delete(TEntity entity)
        {
            BeforeDelete(entity);
            context.Set<TEntity>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public Task<TEntity> GetByIdAsync(TEntityKey id)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            query = AddIncludes(query);

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public Task<TEntity> GetByIdAsync(TEntityKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            query = AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(RSearchParams searchParams)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            query = AddIncludes(query);
            query = AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, ConvertIdToBase64, ConvertBase64ToIdType, AddAfterExp, AddBeforeExp);
        }

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(IQueryable<TEntity> query, RSearchParams searchParams)
        {
            query = AddIncludes(query);
            query = AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, ConvertIdToBase64, ConvertBase64ToIdType, AddAfterExp, AddBeforeExp);
        }

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(RSearchParams searchParams, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            query = AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            query = AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, ConvertIdToBase64, ConvertBase64ToIdType, AddAfterExp, AddBeforeExp);
        }

        protected virtual IQueryable<TEntity> AddWhereClauses(IQueryable<TEntity> query, RSearchParams searchParams)
        {
            return query;
        }

        protected virtual void BeforeDelete(TEntity entity) { }

        protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
        {
            return query;
        }
    }

    public abstract class Repository<TEntity, RSearchParams> : Repository<TEntity, int, RSearchParams>
        where TEntity : class, IIdentifiable<int>
        where RSearchParams : CursorPaginationParameters
    {
        public Repository(DataContext context) : base(
            context,
            Id => Convert.ToBase64String(BitConverter.GetBytes(Id)),
            str =>
            {
                try
                {
                    return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
                }
                catch
                {
                    throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
                }
            },
            (source, afterId) => source.Where(item => item.Id > afterId),
            (source, beforeId) => source.Where(item => item.Id < beforeId)
        )
        { }
    }
}