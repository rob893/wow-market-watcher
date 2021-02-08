using System;

namespace WoWMarketWatcher.Common.Models.Requests
{
    public record EditRoleRequest
    {
        public string[] RoleNames { get; init; } = Array.Empty<string>();
    }
}