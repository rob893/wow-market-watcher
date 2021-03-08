using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.UI.Services;

namespace WoWMarketWatcher.UI.Pages
{
    public partial class WatchList
    {
        [Parameter]
        public int WatchListId { get; set; }

        [Inject]
        private AuctionTimeSeriesService AuctionTimeSeriesService { get; set; }

        [Inject]
        private IAuthService AuthService { get; set; }

        [Inject]
        private IWatchListService WatchListService { get; set; }

        private WatchListDto watchList;

        private readonly Dictionary<int, List<AuctionTimeSeriesEntryDto>> auctionTimeSeries = new();
        private readonly Dictionary<int, List<ChartSeries>> auctionTimeSeriesChartSeries = new();
        private readonly Dictionary<int, string[]> auctionTimeSeriesXAxisLabel = new();
        private readonly Dictionary<int, WoWItemDto> wowItems = new();

        protected override async Task OnInitializedAsync()
        {
            var userId = this.AuthService.LoggedInUser.Id;
            this.watchList = (await this.WatchListService.GetWatchListsForUserAsync(userId)).FirstOrDefault(list => list.Id == this.WatchListId);

            var timeSeriesTasks = new List<Task<List<AuctionTimeSeriesEntryDto>>>();

            foreach (var item in this.watchList.WatchedItems)
            {
                timeSeriesTasks.Add(this.AuctionTimeSeriesService.GetAuctionTimeSeriesAsync(new AuctionTimeSeriesQueryParameters
                {
                    WoWItemId = item.Id,
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    IncludeEdges = false
                }));

                this.auctionTimeSeries[item.Id] = new List<AuctionTimeSeriesEntryDto>();
                this.wowItems[item.Id] = item;
            }

            var completed = await Task.WhenAll(timeSeriesTasks);

            foreach (var list in completed)
            {
                foreach (var timeEntry in list)
                {
                    this.auctionTimeSeries[timeEntry.WoWItemId].Add(timeEntry);
                }
            }

            foreach (var entry in this.auctionTimeSeries)
            {
                this.auctionTimeSeriesChartSeries[entry.Key] = new List<ChartSeries>
                {

                    new ChartSeries
                    {
                        Name = "25th Percentile",
                        Data = entry.Value.Select(v => (double)(v.Price25Percentile / 10000)).ToArray()
                    },
                    new ChartSeries
                    {
                        Name = "50th Percentile",
                        Data = entry.Value.Select(v => (double)(v.Price50Percentile / 10000)).ToArray()
                    },
                    new ChartSeries
                    {
                        Name = "75th Percentile",
                        Data = entry.Value.Select(v => (double)(v.Price75Percentile / 10000)).ToArray()
                    },
                    new ChartSeries
                    {
                        Name = "95th Percentile",
                        Data = entry.Value.Select(v => (double)(v.Price95Percentile / 10000)).ToArray()
                    }
                };
                this.auctionTimeSeriesXAxisLabel[entry.Key] = entry.Value.Select(v => v.Timestamp.ToLocalTime().ToString("M")).Take(2).ToArray();
            }
        }
    }
}