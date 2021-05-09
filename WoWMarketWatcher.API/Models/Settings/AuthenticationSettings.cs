using System.Collections.Generic;
namespace WoWMarketWatcher.API.Models.Settings
{
    public record AuthenticationSettings
    {
        /// <summary>
        /// The API secret of this API
        /// </summary>
        public string APISecrect { get; init; } = default!;

        /// <summary>
        /// The value of the audience claim for tokens created by this API
        /// </summary>
        public string TokenAudience { get; init; } = default!;

        /// <summary>
        /// The value of the issuer claim for tokens created by this API
        /// </summary>
        public string TokenIssuer { get; init; } = default!;

        /// <summary>
        /// This is the amount of time a token issued by this API FOR THIS API (not tokens issued from the API for other APIs using the TokensController) is good for.
        /// For example, on ad authentication, a token for use by this API is created and should have a short lifespan so the calling API can then use that token 
        /// to call the TokensController with to generate a token for its use.
        /// </summary>
        public int TokenExpirationTimeInMinutes { get; init; }

        public int RefreshTokenExpirationTimeInMinutes { get; init; }

        /// <summary>
        /// List of Google OAuth Client IDs to validate Google Tokens against
        /// </summary>
        public List<string> GoogleOAuthAudiences { get; init; } = new List<string>();

        public string ForgotPasswordCallbackUrl { get; init; } = default!;

        public string ConfirmEmailCallbackUrl { get; init; } = default!;
    }
}