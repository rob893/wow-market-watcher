using System;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.DTOs.Users
{
    public record UserDto : IIdentifiable<int>
    {
        public int Id { get; init; }

        public string UserName { get; init; } = default!;

        public string? FirstName { get; init; }

        public string? LastName { get; init; }

        public string Email { get; init; } = default!;

        public UserPreferenceDto Preferences { get; init; } = default!;

        public MembershipLevel MembershipLevel { get; init; }

        public DateTimeOffset Created { get; init; }

        public List<string> Roles { get; init; } = new();

        public List<LinkedAccountDto> LinkedAccounts { get; init; } = new();
    }
}