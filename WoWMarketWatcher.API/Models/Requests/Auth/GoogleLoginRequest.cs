using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Auth
{
    public record GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; init; } = default!;

        [Required]
        public string DeviceId { get; init; } = default!;
    }
}