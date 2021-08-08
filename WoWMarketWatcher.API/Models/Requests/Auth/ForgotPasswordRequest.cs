using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Auth
{
    public record ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
    }
}