using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Requests.Users
{
    public record UpdateUserRequest
    {
        [MaxLength(255)]
        public string? FirstName { get; init; }

        [MaxLength(255)]
        public string? LastName { get; init; }
    }
}