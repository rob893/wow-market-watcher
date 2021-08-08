namespace WoWMarketWatcher.API.Models.Entities
{
    public enum AlertConditionMetric
    {
        TotalAvailableForAuction,
        AveragePrice,
        MinPrice,
        MaxPrice,
        Price25Percentile,
        Price50Percentile,
        Price75Percentile,
        Price95Percentile,
        Price99Percentile
    }
}