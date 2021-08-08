using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.Entities
{
    public class AlertAction : IIdentifiable<int>
    {
        public int Id { get; init; }

        public int AlertId { get; init; }

        public Alert Alert { get; init; } = default!;

        [MaxLength(30)]
        public AlertActionType Type { get; set; }

        [MaxLength(50)]
        public string Target { get; set; } = default!;
    }
}