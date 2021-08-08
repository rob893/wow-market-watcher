using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Entities;

namespace WoWMarketWatcher.API.Services
{
    public interface IAlertService
    {
        Task<bool> EvaluateAlertAsync(Alert alert);
    }
}