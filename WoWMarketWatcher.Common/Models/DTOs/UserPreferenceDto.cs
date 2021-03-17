namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record UserPreferenceDto : IIdentifiable<int>, IOwnedByUser<int>
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public UITheme UITheme { get; init; }
    }
}