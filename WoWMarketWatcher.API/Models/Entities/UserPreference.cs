using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class UserPreference : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;

        [MaxLength(20)]
        public UITheme UITheme { get; set; } = UITheme.Dark;
    }
}