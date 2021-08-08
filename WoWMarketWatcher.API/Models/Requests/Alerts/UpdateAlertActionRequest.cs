using System.ComponentModel.DataAnnotations;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Models.Requests.Alerts
{
    public record UpdateAlertActionRequest
    {
        public AlertActionType? Type { get; init; }

        [MaxLength(50)]
        public string? Target { get; init; }
    }
}