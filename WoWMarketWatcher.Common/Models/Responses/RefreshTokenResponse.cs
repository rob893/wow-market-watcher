namespace WoWMarketWatcher.Common.Models.Responses
{
    public record RefreshTokenResponse
    {
        public string Token { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
    }
}