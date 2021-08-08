using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class LinkedAccount : IIdentifiable<string>, IOwnedByUser<int>
    {
        [MaxLength(255)]
        public string Id { get; set; } = default!;

        [MaxLength(50)]
        [JsonConverter(typeof(StringEnumConverter))]
        public LinkedAccountType LinkedAccountType { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = default!;
    }
}