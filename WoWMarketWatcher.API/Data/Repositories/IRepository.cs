using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IRepository<TEntity, TEntityKey, TSearchParams>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        where TSearchParams : CursorPaginationQueryParameters
    {
        void Add(TEntity entity);

        void AddRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

        Task<bool> SaveAllAsync();

        Task<int> SaveChangesAsync();

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> condition, bool track = true);

        Task<TEntity> GetByIdAsync(TEntityKey id, bool track = true);

        Task<TEntity> GetByIdAsync(TEntityKey id, params Expression<Func<TEntity, object>>[] includes);

        Task<List<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> condition, bool track = true);

        Task<CursorPaginatedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, bool track = true);
    }

    public interface IRepository<TEntity, TSearchParams> : IRepository<TEntity, int, TSearchParams>
        where TEntity : class, IIdentifiable<int>
        where TSearchParams : CursorPaginationQueryParameters
    { }
}