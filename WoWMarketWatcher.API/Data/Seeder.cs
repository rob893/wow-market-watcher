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
                SeedRoles();
                SeedUsers();
            }
        }

        private void ClearAllData()
        {
            context.RefreshTokens.Clear();
            context.Users.Clear();
            context.Roles.Clear();

            context.SaveChanges();
        }

        private void SeedRoles()
        {
            if (context.Roles.Any())
            {
                return;
            }

            string data = File.ReadAllText("Data/SeedData/RoleSeedData.json");
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

            string data = File.ReadAllText("Data/SeedData/UserSeedData.json");
            List<User> users = JsonConvert.DeserializeObject<List<User>>(data);

            foreach (User user in users)
            {
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
    }
}