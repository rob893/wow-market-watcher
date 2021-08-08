using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Data.Repositories
{
    public sealed class UserRepository : Repository<User, CursorPaginationQueryParameters>, IUserRepository
    {
        public UserManager<User> UserManager { get; init; }

        private readonly SignInManager<User> signInManager;


        public UserRepository(DataContext context, UserManager<User> userManager, SignInManager<User> signInManager) : base(context)
        {
            this.UserManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateUserWithAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Created = DateTime.UtcNow;
            var created = await this.UserManager.CreateAsync(user);
            await this.UserManager.AddToRoleAsync(user, "User");

            return created;
        }

        public async Task<IdentityResult> CreateUserWithPasswordAsync(User user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Created = DateTime.UtcNow;
            var created = await this.UserManager.CreateAsync(user, password);
            await this.UserManager.AddToRoleAsync(user, "User");

            return created;
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            IQueryable<User> query = this.Context.Users;
            query = this.AddIncludes(query);

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<User?> GetByLinkedAccountAsync(string id, LinkedAccountType accountType, params Expression<Func<User, object>>[] includes)
        {
            var linkedAccount = await this.Context.LinkedAccounts.FirstOrDefaultAsync(account => account.Id == id && account.LinkedAccountType == accountType);

            if (linkedAccount == null)
            {
                return null;
            }

            IQueryable<User> query = this.Context.Users;
            query = this.AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return await query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.Id == linkedAccount.UserId);
        }

        public Task<User> GetByUsernameAsync(string username, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = this.Context.Users;

            query = this.AddIncludes(query);
            query = includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return query.OrderBy(e => e.Id).FirstOrDefaultAsync(user => user.UserName == username);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            var result = await this.signInManager.CheckPasswordSignInAsync(user, password, false);

            return result.Succeeded;
        }

        public Task<CursorPaginatedList<Role, int>> GetRolesAsync(CursorPaginationQueryParameters searchParams)
        {
            IQueryable<Role> query = this.Context.Roles;

            return query.ToCursorPaginatedListAsync(searchParams);
        }

        public Task<List<Role>> GetRolesAsync()
        {
            return this.Context.Roles.ToListAsync();
        }

        protected override IQueryable<User> AddIncludes(IQueryable<User> query)
        {
            return query
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .Include(user => user.LinkedAccounts)
                .Include(user => user.Preferences);
        }
    }
}