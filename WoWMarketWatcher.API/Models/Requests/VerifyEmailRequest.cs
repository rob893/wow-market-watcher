using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record ConfirmEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
        [Required]
        public string Token { get; init; } = default!;
    }
}