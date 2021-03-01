using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.UI.Services;

namespace WoWMarketWatcher.UI.Pages
{
    public partial class WatchLists
    {
        [Inject]
        private IWatchListService WatchListService { get; set; }

        [Inject]
        private IAuthService AuthService { get; set; }

        private List<WatchListDto> watchLists = new();

        protected override async Task OnInitializedAsync()
        {
            var userId = this.AuthService.LoggedInUser.Id;
            this.watchLists = await this.WatchListService.GetWatchListsForUserAsync(userId);
        }
    }
}