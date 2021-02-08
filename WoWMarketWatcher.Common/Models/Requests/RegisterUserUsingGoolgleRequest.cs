using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
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