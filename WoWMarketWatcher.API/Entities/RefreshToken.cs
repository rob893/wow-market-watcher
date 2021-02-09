using System;
using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class RefreshToken : IOwnedByUser<int>
    {
        public string DeviceId { get; set; } = default!;
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        [Required]
        [MaxLength(255)]
        public string Token { get; set; } = default!;
        public DateTimeOffset Expiration { get; set; }
    }
}