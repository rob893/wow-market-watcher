using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class User : IdentityUser<int>, IIdentifiable<int>
    {
        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? LastName { get; set; }

        public DateTimeOffset Created { get; set; }

        [MaxLength(50)]
        public MembershipLevel MembershipLevel { get; set; }

        public UserPreference Preferences { get; set; } = default!;

        public List<RefreshToken> RefreshTokens { get; set; } = new();

        public List<UserRole> UserRoles { get; set; } = new();

        public List<LinkedAccount> LinkedAccounts { get; set; } = new();

        public List<WatchList> WatchLists { get; set; } = new();

        public List<Alert> Alerts { get; set; } = new();
    }
}