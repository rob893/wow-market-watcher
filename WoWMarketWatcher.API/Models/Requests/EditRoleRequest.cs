using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record EditRoleRequest
    {
        public List<string> RoleNames { get; init; } = new List<string>();
    }
}