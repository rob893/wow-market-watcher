using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Auth
{
    public record RegisterUserUsingGoolgleRequest
    {
        [Required]
        public string IdToken { get; init; } = default!;

        [Required]
        public string UserName { get; init; } = default!;

        [Required]
        public string DeviceId { get; init; } = default!;
    }
}