using WoWMarketWatcher.API.Models.DTOs.Users;

namespace WoWMarketWatcher.API.Models.Responses.Auth
{
    public record LoginResponse
    {
        public string Token { get; init; } = default!;

        public string RefreshToken { get; init; } = default!;

        public UserDto User { get; init; } = default!;
    }
}