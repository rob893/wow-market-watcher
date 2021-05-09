using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
    }
}