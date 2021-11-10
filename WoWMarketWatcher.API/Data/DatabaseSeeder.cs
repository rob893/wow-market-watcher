using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Data
{
    public sealed class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly DataContext context;

        private readonly UserManager<User> userManager;

        private readonly RoleManager<Role> roleManager;

        public DatabaseSeeder(DataContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public void SeedDatabase(bool seedData, bool clearCurrentData, bool applyMigrations, bool dropDatabase)
        {
            if (dropDatabase)
            {
                this.context.Database.EnsureDeleted();
            }

            if (applyMigrations)
            {
                this.context.Database.Migrate();
            }

            if (clearCurrentData)
            {
                this.ClearAllData();
            }

            if (seedData)
            {
                this.SeedWoWItems();
                this.SeedRealms();
                this.SeedRoles();
                this.SeedUsers();
                this.SeedAuctionTimeSeries();

                this.context.SaveChanges();
            }
        }

        private void ClearAllData()
        {
            this.context.RefreshTokens.Clear();
            this.context.Users.Clear();
            this.context.Roles.Clear();
            this.context.AuctionTimeSeries.Clear();
            this.context.WoWItems.Clear();
            this.context.Realms.Clear();
            this.context.ConnectedRealms.Clear();

            this.context.SaveChanges();
        }

        private void SeedRoles()
        {
            if (this.context.Roles.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/RoleSeedData.json");
            var roles = JsonConvert.DeserializeObject<List<Role>>(data);

            if (roles == null)
            {
                throw new JsonException("Unable to deserialize data.");
            }

            foreach (var role in roles)
            {
                this.roleManager.CreateAsync(role).Wait();
            }
        }

        private void SeedUsers()
        {
            if (this.userManager.Users.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(data);

            if (users == null)
            {
                throw new JsonException("Unable to deserialize data.");
            }

            foreach (var user in users)
            {
                this.userManager.CreateAsync(user, "password").Wait();

                if (user.UserName.ToUpperInvariant() == "ADMIN")
                {
                    this.userManager.AddToRoleAsync(user, "Admin").Wait();
                    this.userManager.AddToRoleAsync(user, "User").Wait();
                }
                else
                {
                    this.userManager.AddToRoleAsync(user, "User").Wait();
                }
            }
        }

        private void SeedWoWItems()
        {
            if (this.context.WoWItems.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/WoWItemsSeedData.json");
            var items = JsonConvert.DeserializeObject<List<WoWItem>>(data);

            if (items == null)
            {
                throw new JsonSerializationException("Unable to deserialize json string.");
            }

            this.context.WoWItems.AddRange(items);
        }

        private void SeedRealms()
        {
            if (this.context.WoWItems.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/ConnectedRealmsSeedData.json");
            var items = JsonConvert.DeserializeObject<List<ConnectedRealm>>(data);

            if (items == null)
            {
                throw new JsonSerializationException("Unable to deserialize json string.");
            }

            this.context.ConnectedRealms.AddRange(items);
        }

        private void SeedAuctionTimeSeries()
        {
            if (this.context.AuctionTimeSeries.Any())
            {
                return;
            }

            var data = File.ReadAllText("Data/SeedData/AuctionTimeSeriesSeedData.json");
            var items = JsonConvert.DeserializeObject<List<AuctionTimeSeriesEntry>>(data);

            if (items == null)
            {
                throw new JsonSerializationException("Unable to deserialize json string.");
            }

            this.context.AuctionTimeSeries.AddRange(items);
        }
    }
}