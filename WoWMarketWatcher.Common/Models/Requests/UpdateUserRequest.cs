using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record UpdateUserRequest
    {
        [MaxLength(255)]
        public string? FirstName { get; init; }
        [MaxLength(255)]
        public string? LastName { get; init; }
    }
}