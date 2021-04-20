using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class User : IdentityUser<int>, IIdentifiable<int>
    {
        [MaxLength(255)]
        public string? FirstName { get; set; }
        [MaxLength(255)]
        public string? LastName { get; set; }
        public DateTimeOffset Created { get; set; }
        public MembershipLevel MembershipLevel { get; set; }
        public UserPreference Preferences { get; set; } = default!;
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public List<LinkedAccount> LinkedAccounts { get; set; } = new List<LinkedAccount>();
        public List<WatchList> WatchLists { get; set; } = new List<WatchList>();
    }
}