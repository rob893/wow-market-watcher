using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WoWMarketWatcher.Common.Models.DTOs
{
    public record LinkedAccountDto : IIdentifiable<string>, IOwnedByUser<int>
    {
        public string Id { get; init; } = default!;
        [JsonConverter(typeof(StringEnumConverter))]
        public LinkedAccountType LinkedAccountType { get; set; }
        public int UserId { get; set;  }
}

}