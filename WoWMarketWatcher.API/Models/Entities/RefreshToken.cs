using System;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class RefreshToken : IOwnedByUser<int>
    {
        [MaxLength(255)]
        public string DeviceId { get; set; } = default!;

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        [Required]
        [MaxLength(255)]
        public string Token { get; set; } = default!;

        public DateTimeOffset Expiration { get; set; }
    }
}