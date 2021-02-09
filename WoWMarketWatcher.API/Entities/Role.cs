using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class Role : IdentityRole<int>, IIdentifiable<int>
    {
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}