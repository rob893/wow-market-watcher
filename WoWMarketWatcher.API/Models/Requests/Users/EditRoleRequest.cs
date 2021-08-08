using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Requests.Users
{
    public record EditRoleRequest
    {
        public List<string> RoleNames { get; init; } = new();
    }
}