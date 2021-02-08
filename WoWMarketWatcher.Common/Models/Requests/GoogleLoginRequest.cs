using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; init; } = default!;
        [Required]
        public string DeviceId { get; init; } = default!;
    }
}