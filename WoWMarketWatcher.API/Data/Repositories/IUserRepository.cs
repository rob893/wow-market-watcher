using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public interface IUserRepository : IRepository<User, CursorPaginationParameters>
    {
        Task<IdentityResult> CreateUserWithAsync(User user);
        Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password);
        Task<User> GetByUsernameAsync(string username);
        Task<User?> GetByLinkedAccountAsync(string id, LinkedAccountType accountType, params Expression<Func<User, object>>[] includes);
        Task<User> GetByUsernameAsync(string username, params Expression<Func<User, object>>[] includes);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<CursorPagedList<Role, int>> GetRolesAsync(CursorPaginationParameters searchParams);
        Task<List<Role>> GetRolesAsync();
    }
}