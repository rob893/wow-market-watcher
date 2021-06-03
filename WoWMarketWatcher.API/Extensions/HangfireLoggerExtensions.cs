using System;
using System.Collections.Generic;
using Hangfire.JobsLogger;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Extensions;

namespace WoWMarketWatcher.API.Extensions
{
    public static class HangfireLoggerExtensions
    {
        public static void LogTrace(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Trace, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void LogDebug(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Debug, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void LogInformation(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Information, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void LogWarning(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Warning, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void LogError(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Error, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void LogCritical(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Critical, hangfireJobId, sourceName, correlationId, message, customProperties);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, string hangfireJobId, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (customProperties == null)
            {
                customProperties = new Dictionary<string, object>();
            }

            customProperties[nameof(sourceName)] = sourceName;
            customProperties[nameof(correlationId)] = correlationId;
            customProperties[nameof(hangfireJobId)] = hangfireJobId;

            using (logger.BeginScope(customProperties))
            {
                logger.Log(logLevel, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message));
            }
        }

        private static string FormatMessage(string sourceName, string correlationId, string message)
        {
            return $"{sourceName} ({correlationId}). {message.TrimEnd('.')}.";
        }
    }
}