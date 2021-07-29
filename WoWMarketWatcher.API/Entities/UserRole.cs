using Microsoft.AspNetCore.Identity;

namespace WoWMarketWatcher.API.Entities
{
    public class UserRole : IdentityUserRole<int>
    {
        public User User { get; set; } = default!;

        public Role Role { get; set; } = default!;
    }
}