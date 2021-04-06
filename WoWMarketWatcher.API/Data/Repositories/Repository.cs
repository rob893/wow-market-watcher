using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public abstract class Repository<TEntity, TEntityKey, TSearchParams> : IRepository<TEntity, TEntityKey, TSearchParams>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        where TSearchParams : CursorPaginationParameters
    {
        public DataContext Context { get; protected init; }

        protected Func<TEntityKey, string> ConvertIdToBase64 { get; init; }
        protected Func<string, TEntityKey> ConvertBase64ToIdType { get; init; }
        protected Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddAfterExp { get; init; }
        protected Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddBeforeExp { get; init; }

        public Repository(DataContext context, Func<TEntityKey, string> ConvertIdToBase64, Func<string, TEntityKey> ConvertBase64ToIdType,
            Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddAfterExp, Func<IQueryable<TEntity>, TEntityKey, IQueryable<TEntity>> AddBeforeExp)
        {
            this.Context = context;
            this.ConvertIdToBase64 = ConvertIdToBase64;
            this.ConvertBase64ToIdType = ConvertBase64ToIdType;
            this.AddAfterExp = AddAfterExp;
            this.AddBeforeExp = AddBeforeExp;
        }

        public EntityEntry<TEntity> Entry(TEntity entity)
        {
            return this.Context.Entry(entity);
        }

        public void Add(TEntity entity)
        {
            this.Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            this.Context.Set<TEntity>().AddRange(entities);
        }

        public void Delete(TEntity entity)
        {
            this.BeforeDelete(entity);
            this.Context.Set<TEntity>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                this.BeforeDelete(entity);
            }

            this.Context.Set<TEntity>().RemoveRange(entities);
        }

        public IQueryable<TEntity> EntitySetAsNoTracking()
        {
            return this.Context.Set<TEntity>().AsNoTracking();
        }

        public IQueryable<TEntity> EntitySet()
        {
            return this.Context.Set<TEntity>();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.Context.SaveChangesAsync() > 0;
        }

        public Task<int> SaveChangesAsync()
        {
            return this.Context.SaveChangesAsync();
        }

        public Task<TEntity> GetByIdAsync(TEntityKey id)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

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

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            query = this.AddIncludes(query);
            query = this.AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, this.ConvertIdToBase64, this.ConvertBase64ToIdType, this.AddAfterExp, this.AddBeforeExp);
        }

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(IQueryable<TEntity> query, TSearchParams searchParams)
        {
            query = this.AddIncludes(query);
            query = this.AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, this.ConvertIdToBase64, this.ConvertBase64ToIdType, this.AddAfterExp, this.AddBeforeExp);
        }

        public Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = this.Context.Set<TEntity>();

            query = this.AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            query = this.AddWhereClauses(query, searchParams);

            return CursorPagedList<TEntity, TEntityKey>.CreateAsync(query, searchParams, this.ConvertIdToBase64, this.ConvertBase64ToIdType, this.AddAfterExp, this.AddBeforeExp);
        }

        protected virtual IQueryable<TEntity> AddWhereClauses(IQueryable<TEntity> query, TSearchParams searchParams)
        {
            return query;
        }

        protected virtual void BeforeDelete(TEntity entity) { }

        protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
        {
            return query;
        }
    }

    public abstract class Repository<TEntity, TSearchParams> : Repository<TEntity, int, TSearchParams>, IRepository<TEntity, int, TSearchParams>
        where TEntity : class, IIdentifiable<int>
        where TSearchParams : CursorPaginationParameters
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