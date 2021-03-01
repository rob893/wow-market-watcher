using System.Collections.Generic;
using System.Threading.Tasks;
using WoWMarketWatcher.Common.Models.DTOs;

namespace WoWMarketWatcher.UI.Services
{
    public interface IWatchListService
    {
        Task<List<WatchListDto>> GetWatchListsForUserAsync(int userId, string correlationId = null);
    }
}