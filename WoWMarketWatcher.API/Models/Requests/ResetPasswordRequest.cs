using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record ResetPasswordRequest
    {
        [Required]
        public string Password { get; init; } = default!;
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
        [Required]
        public string Token { get; init; } = default!;
    }
}