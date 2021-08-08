namespace WoWMarketWatcher.API.Models.DTOs.Users
{
    public record LinkedAccountDto : IIdentifiable<string>, IOwnedByUser<int>
    {
        public string Id { get; init; } = default!;

        public LinkedAccountType LinkedAccountType { get; set; }

        public int UserId { get; set; }
    }

}