using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Auth
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