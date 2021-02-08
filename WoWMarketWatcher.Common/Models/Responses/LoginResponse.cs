namespace WoWMarketWatcher.Common.Models.Responses
{
    public record LoginResponse
    {
        public string Token { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
        public UserDto User { get; init; } = default!;
    }
}