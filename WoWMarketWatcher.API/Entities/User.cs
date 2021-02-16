using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class User : IdentityUser<int>, IIdentifiable<int>
    {
        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = default!;
        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = default!;
        public DateTimeOffset Created { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public List<LinkedAccount> LinkedAccounts { get; set; } = new List<LinkedAccount>();
        public List<WatchList> WatchLists { get; set; } = new List<WatchList>();
    }
}