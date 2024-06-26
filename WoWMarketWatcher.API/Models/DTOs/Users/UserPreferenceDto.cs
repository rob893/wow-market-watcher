using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.DTOs.Users
{
    public record UserPreferenceDto : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }

        public int UserId { get; init; }

        public UITheme UITheme { get; init; }
    }
}