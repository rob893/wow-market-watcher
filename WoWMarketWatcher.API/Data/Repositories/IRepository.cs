using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.Common.Models;
using WoWMarketWatcher.Common.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IRepository<TEntity, TEntityKey, TSearchParams>
        where TEntity : class, IIdentifiable<TEntityKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
        where TSearchParams : CursorPaginationParameters
    {
        EntityEntry<TEntity> Entry(TEntity entity);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        IQueryable<TEntity> EntitySetAsNoTracking();
        IQueryable<TEntity> EntitySet();
        Task<bool> SaveAllAsync();
        Task<int> SaveChangesAsync();
        Task<TEntity> GetByIdAsync(TEntityKey id);
        Task<TEntity> GetByIdAsync(TEntityKey id, params Expression<Func<TEntity, object>>[] includes);
        Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams);
        Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(IQueryable<TEntity> query, TSearchParams searchParams);
        Task<CursorPagedList<TEntity, TEntityKey>> SearchAsync(TSearchParams searchParams, params Expression<Func<TEntity, object>>[] includes);
    }

    public interface IRepository<TEntity, TSearchParams> : IRepository<TEntity, int, TSearchParams>
        where TEntity : class, IIdentifiable<int>
        where TSearchParams : CursorPaginationParameters
    { }
}