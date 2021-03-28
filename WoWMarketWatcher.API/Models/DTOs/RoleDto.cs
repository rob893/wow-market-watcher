namespace WoWMarketWatcher.API.Models.DTOs
{
    public record RoleDto : IIdentifiable<int>
    {
        public int Id { get; init; }
        public string Name { get; init; } = default!;
        public string NormalizedName { get; init; } = default!;
    }
}