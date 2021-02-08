using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record RefreshTokenRequest
    {
        [Required]
        public string Token { get; init; } = default!;
        [Required]
        public string RefreshToken { get; init; } = default!;
        [Required]
        public string DeviceId { get; init; } = default!;
    }
}