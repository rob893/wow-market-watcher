using System.Threading.Tasks;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Responses;

namespace WoWMarketWatcher.UI.Services
{
    public interface IAuthService
    {
        string AccessToken { get; }
        UserDto LoggedInUser { get; }
        Task<LoginResponse> Login(string username, string password);
        Task<RefreshTokenResponse> RefreshTokenAsync();
    }
}