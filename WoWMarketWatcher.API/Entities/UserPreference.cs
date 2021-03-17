using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Entities
{
    public class UserPreference : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public UITheme UITheme { get; set; } = UITheme.Dark;
    }
}