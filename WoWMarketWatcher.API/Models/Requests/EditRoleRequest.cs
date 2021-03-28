using System;

namespace WoWMarketWatcher.API.Models.Requests
{
    public record EditRoleRequest
    {
        public string[] RoleNames { get; init; } = Array.Empty<string>();
    }
}