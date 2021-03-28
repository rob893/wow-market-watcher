using System;
using System.Collections.Generic;

namespace WoWMarketWatcher.API.Models.DTOs
{
    public record UserDto : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string UserName { get; init; } = default!;
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public UserPreferenceDto Preferences { get; init; } = default!;
        public MembershipLevel MembershipLevel { get; init; }
        public DateTimeOffset Created { get; init; }
        public IEnumerable<string> Roles { get; init; } = new List<string>();
        public IEnumerable<LinkedAccountDto> LinkedAccounts { get; init; } = new List<LinkedAccountDto>();
    }
}