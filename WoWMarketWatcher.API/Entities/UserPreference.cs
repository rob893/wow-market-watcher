using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Entities
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