using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Services
{
    public interface IJwtTokenService
    {
        Task<(bool, User?)> IsTokenEligibleForRefreshAsync(string token, string refreshToken, string deviceId);

        Task<string> GenerateAndSaveRefreshTokenForUserAsync(User user, string deviceId);

        string GenerateJwtTokenForUser(User user);
    }
}