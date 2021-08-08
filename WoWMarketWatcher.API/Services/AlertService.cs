using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;

namespace WoWMarketWatcher.API.Services
{
    public sealed class AlertService : IAlertService
    {
        private readonly IAuctionTimeSeriesRepository auctionTimeSeriesRepository;

        private readonly IAlertRepository alertRepository;

        private readonly IEmailService emailService;

        public AlertService(
            IAuctionTimeSeriesRepository auctionTimeSeriesRepository,
            IAlertRepository alertRepository,
            IEmailService emailService)
        {
            this.auctionTimeSeriesRepository = auctionTimeSeriesRepository ?? throw new ArgumentNullException(nameof(auctionTimeSeriesRepository));
            this.alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        public async Task<bool> EvaluateAlertAsync(Alert alert)
        {
            if (alert == null)
            {
                throw new ArgumentNullException(nameof(alert));
            }

            if (alert.Conditions == null || alert.Conditions.Count == 0)
            {
                return false;
            }

            var earliestDate = DateTime.UtcNow.AddHours(-alert.Conditions.Select(condition => condition.AggregationTimeGranularityInHours).Max());

            var entriesToEvaluate = await this.auctionTimeSeriesRepository
                .SearchAsync(entry => entry.WoWItemId == alert.WoWItemId && entry.ConnectedRealmId == alert.ConnectedRealmId && entry.Timestamp >= earliestDate);
            var orderedEntriesToEvaludate = entriesToEvaluate.OrderBy(entry => entry.Timestamp);

            var shouldFireAlert = ShouldFireAlert(alert, orderedEntriesToEvaludate);
            var now = DateTime.UtcNow;

            if (shouldFireAlert)
            {
                await this.ProcessAlertActionsAsync(alert);
                alert.LastFired = now;
            }

            alert.LastEvaluated = now;

            await this.alertRepository.SaveChangesAsync();

            return shouldFireAlert;
        }

        private static bool ShouldFireAlert(Alert alert, IEnumerable<AuctionTimeSeriesEntry> auctionTimeSeries)
        {
            return alert.Conditions.All(condition => EvaluateCondition(condition, auctionTimeSeries));
        }

        private static bool EvaluateCondition(AlertCondition condition, IEnumerable<AuctionTimeSeriesEntry> auctionTimeSeries)
        {
            var entriesToEvaluate = auctionTimeSeries.Where(entry => entry.Timestamp >= DateTime.UtcNow.AddHours(-condition.AggregationTimeGranularityInHours));
            var aggregation = AggregateMetricValues(condition.AggregationType, entriesToEvaluate.Select(entry => GetMetricValue(condition.Metric, entry)));

            return CompareMetric(condition.Operator, aggregation, condition.Threshold);
        }

        private static long AggregateMetricValues(AlertConditionAggregationType type, IEnumerable<long> metricValues)
        {
            return type switch
            {
                AlertConditionAggregationType.Sum => metricValues.Sum(),
                AlertConditionAggregationType.Count => metricValues.Count(),
                AlertConditionAggregationType.Average => (long)metricValues.Average(),
                AlertConditionAggregationType.Min => metricValues.Min(),
                AlertConditionAggregationType.Max => metricValues.Max(),
                _ => throw new ArgumentException($"Aggregation type {type} is not supported.", nameof(type)),
            };
        }

        private static long GetMetricValue(AlertConditionMetric metric, AuctionTimeSeriesEntry entry)
        {
            return metric switch
            {
                AlertConditionMetric.AveragePrice => entry.AveragePrice,
                AlertConditionMetric.MinPrice => entry.MinPrice,
                AlertConditionMetric.MaxPrice => entry.MaxPrice,
                AlertConditionMetric.Price25Percentile => entry.Price25Percentile,
                AlertConditionMetric.Price50Percentile => entry.Price50Percentile,
                AlertConditionMetric.Price75Percentile => entry.Price75Percentile,
                AlertConditionMetric.Price95Percentile => entry.Price95Percentile,
                AlertConditionMetric.Price99Percentile => entry.Price99Percentile,
                AlertConditionMetric.TotalAvailableForAuction => entry.TotalAvailableForAuction,
                _ => throw new ArgumentException($"{metric} is not a supported metric.", nameof(metric))
            };
        }

        private static bool CompareMetric(AlertConditionOperator conditionOperator, long metricValue, long metricThreshold)
        {
            return conditionOperator switch
            {
                AlertConditionOperator.GreaterThan => metricValue > metricThreshold,
                AlertConditionOperator.GreaterThanOrEqualTo => metricValue >= metricThreshold,
                AlertConditionOperator.LessThan => metricValue < metricThreshold,
                AlertConditionOperator.LessThanOrEqualTo => metricValue <= metricThreshold,
                _ => throw new ArgumentException($"{conditionOperator} is not a supported operator.", nameof(conditionOperator)),
            };
        }

        private async Task ProcessAlertActionsAsync(Alert alert)
        {
            if (alert.Actions == null || alert.Actions.Count == 0)
            {
                return;
            }

            foreach (var action in alert.Actions)
            {
                if (action.Type == AlertActionType.Email)
                {
                    await this.emailService.SendEmailAsync(action.Target, "Your Alert Fired!", $"Your alert {alert.Name} has fired.");
                }
            }
        }
    }
}