using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Entities;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Services
{
    public sealed class AlertService : IAlertService
    {
        private readonly IAuctionTimeSeriesRepository auctionTimeSeriesRepository;

        private readonly IAlertRepository alertRepository;

        private readonly IEmailService emailService;

        private readonly ICorrelationIdService correlationIdService;

        private readonly ILogger<AlertService> logger;

        public AlertService(
            IAuctionTimeSeriesRepository auctionTimeSeriesRepository,
            IAlertRepository alertRepository,
            IEmailService emailService,
            ICorrelationIdService correlationIdService,
            ILogger<AlertService> logger)
        {
            this.auctionTimeSeriesRepository = auctionTimeSeriesRepository ?? throw new ArgumentNullException(nameof(auctionTimeSeriesRepository));
            this.alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string CorrelationId => this.correlationIdService.CorrelationId;

        public async Task<bool> EvaluateAlertAsync(Alert alert)
        {
            if (alert == null)
            {
                throw new ArgumentNullException(nameof(alert));
            }

            var sourceName = GetSourceName();

            if (alert.Conditions == null || alert.Conditions.Count == 0)
            {
                return false;
            }

            var (isEvaluable, conditionsMet) = await this.ConditionsMetAsync(alert);
            var now = DateTime.UtcNow;
            var oldState = alert.State;

            if (!isEvaluable)
            {
                this.logger.LogInformation(sourceName, this.CorrelationId, $"One or more conditions for alert {alert.Id} lacks recent enough data to evaluate. No state change.");
            }

            if (isEvaluable && conditionsMet && alert.State != AlertState.Alarm)
            {
                await this.ProcessAlertActionsAsync(alert, AlertActionOnType.AlertActivated);
                alert.LastFired = now;
                alert.State = AlertState.Alarm;
                this.logger.LogInformation(sourceName, this.CorrelationId, $"Alert {alert.Id} state changed from {oldState} to {alert.State}.");
            }
            else if (isEvaluable && !conditionsMet && alert.State != AlertState.Ok)
            {
                alert.State = AlertState.Ok;
                this.logger.LogInformation(sourceName, this.CorrelationId, $"Alert {alert.Id} state changed from {oldState} to {alert.State}.");
            }

            alert.LastEvaluated = now;

            await this.alertRepository.SaveChangesAsync();

            this.logger.LogInformation(sourceName, this.CorrelationId, $"Alert {alert.Id} evaluated.");

            return conditionsMet;
        }

        private async Task<(bool IsEvaluable, bool ConditionsMet)> ConditionsMetAsync(Alert alert)
        {
            foreach (var condition in alert.Conditions)
            {
                var (isEvaluable, conditionMet) = await this.EvaluateConditionAsync(condition);

                if (!isEvaluable || !conditionMet)
                {
                    return (isEvaluable, conditionMet);
                }
            }

            return (true, true);
        }

        private async Task<(bool IsEvaluable, bool ConditionMet)> EvaluateConditionAsync(AlertCondition condition)
        {
            var entriesToEvaluate = await this.auctionTimeSeriesRepository
                .SearchAsync(entry =>
                    entry.WoWItemId == condition.WoWItemId &&
                    entry.ConnectedRealmId == condition.ConnectedRealmId &&
                    entry.Timestamp >= DateTime.UtcNow.AddHours(-condition.AggregationTimeGranularityInHours));
            var orderedEntriesToEvaludate = entriesToEvaluate.OrderBy(entry => entry.Timestamp);

            if (!orderedEntriesToEvaludate.Any())
            {
                return (false, false);
            }

            var aggregation = AggregateMetricValues(condition.AggregationType, entriesToEvaluate.Select(entry => GetMetricValue(condition.Metric, entry)));

            return (true, CompareMetric(condition.Operator, aggregation, condition.Threshold));
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

        private async Task ProcessAlertActionsAsync(Alert alert, AlertActionOnType alertActionOnType)
        {
            if (alert.Actions == null || alert.Actions.Count == 0)
            {
                return;
            }

            var tasks = new List<Task>();

            foreach (var action in alert.Actions.Where(a => a.ActionOn == alertActionOnType))
            {
                if (action.Type == AlertActionType.Email)
                {
                    var title = alertActionOnType == AlertActionOnType.AlertActivated
                        ? "Your Alert Fired!"
                        : "Your Alert has deactivated";
                    var message = alertActionOnType == AlertActionOnType.AlertActivated
                        ? $"Your alert {alert.Name} has fired."
                        : $"Your alert {alert.Name} has resolved.";
                    tasks.Add(this.emailService.SendEmailAsync(action.Target, title, message));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}