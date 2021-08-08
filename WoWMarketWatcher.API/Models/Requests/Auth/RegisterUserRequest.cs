using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Auth
{
    public record RegisterUserRequest
    {
        [Required]
        public string UserName { get; init; } = default!;

        [Required]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "You must specify a password between 4 and 256 characters")]
        public string Password { get; init; } = default!;

        [MaxLength(255)]
        public string? FirstName { get; init; }

        [MaxLength(255)]
        public string? LastName { get; init; }

        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;

        [Required]
        public string DeviceId { get; init; } = default!;
    }
}