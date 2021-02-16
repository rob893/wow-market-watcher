using System.Collections.Generic;
using WoWMarketWatcher.API.Entities;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WoWMarketWatcher.API.Extensions;

namespace WoWMarketWatcher.API.Data
{
    public class Seeder
    {
        private readonly DataContext context;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;


        public Seeder(DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void SeedDatabase(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase)
        {
            if (dropDatabase)
            {
                context.Database.EnsureDeleted();
            }

            if (applyMigrations)
            {
                context.Database.Migrate();
            }

            if (clearCurrentData)
            {
                ClearAllData();
            }

            if (seedData)
            {
                SeedWoWItems();
                SeedRealms();
                SeedRoles();
                SeedUsers();
                SeedAuctionTimeSeries();

                context.SaveChanges();
            }
        }

        private void ClearAllData()
        {
            context.RefreshTokens.Clear();
            context.Users.Clear();
            context.Roles.Clear();
            context.AuctionTimeSeries.Clear();
            context.WoWItems.Clear();
            context.Realms.Clear();
            context.ConnectedRealms.Clear();

            context.SaveChanges();
        }

        private void SeedRoles()
        {
            if (context.Roles.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/RoleSeedData.json");
            var roles = JsonConvert.DeserializeObject<List<Role>>(data);

            foreach (var role in roles)
            {
                roleManager.CreateAsync(role).Wait();
            }
        }

        private void SeedUsers()
        {
            if (userManager.Users.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(data);

            foreach (var user in users)
            {
                foreach (var watchList in user.WatchLists)
                {
                    var itemIds = watchList.WatchedItems.Select(i => i.Id).ToHashSet();
                    watchList.WatchedItems.Clear();
                    watchList.WatchedItems.AddRange(context.WoWItems.Where(i => itemIds.Contains(i.Id)));
                }

                userManager.CreateAsync(user, "password").Wait();

                if (user.UserName.ToUpper() == "ADMIN")
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                    userManager.AddToRoleAsync(user, "User").Wait();
                }
                else
                {
                    userManager.AddToRoleAsync(user, "User").Wait();
                }
            }
        }

        private void SeedWoWItems()
        {
            if (context.WoWItems.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/WoWItemsSeedData.json");
            var items = JsonConvert.DeserializeObject<List<WoWItem>>(data);

            context.WoWItems.AddRange(items);
        }

        private void SeedRealms()
        {
            if (context.WoWItems.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/ConnectedRealmsSeedData.json");
            var items = JsonConvert.DeserializeObject<List<ConnectedRealm>>(data);

            context.ConnectedRealms.AddRange(items);
        }

        private void SeedAuctionTimeSeries()
        {
            if (context.WoWItems.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/AuctionTimeSeriesSeedData.json");
            var items = JsonConvert.DeserializeObject<List<AuctionTimeSeriesEntry>>(data);

            context.AuctionTimeSeries.AddRange(items);
        }
    }
}