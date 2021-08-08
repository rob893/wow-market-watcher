using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record CreateAlertActionRequest
    {
        [Required]
        public AlertActionType Type { get; init; }

        [Required]
        [MaxLength(50)]
        public string Target { get; init; } = default!;
    }
}