using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record LoginRequest
    {
        [Required]
        public string Username { get; init; } = default!;
        [Required]
        public string Password { get; init; } = default!;
        [Required]
        public string DeviceId { get; init; } = default!;
    }
}