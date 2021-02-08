using System;
using System.Collections.Generic;

namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record UserDto : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string UserName { get; init; } = default!;
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public DateTimeOffset Created { get; init; }
        public IEnumerable<string> Roles { get; init; } = new List<string>();
        public IEnumerable<LinkedAccountDto> LinkedAccounts { get; init; } = new List<LinkedAccountDto>();
    }
}