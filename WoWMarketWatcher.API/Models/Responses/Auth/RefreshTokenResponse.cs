namespace WoWMarketWatcher.API.Models.Responses.Auth
{
    public record RefreshTokenResponse
    {
        public string Token { get; init; } = default!;

        public string RefreshToken { get; init; } = default!;
    }
}