using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WoWMarketWatcher.API.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogTrace(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Trace, sourceName, correlationId, message, customProperties);
        }

        public static void LogDebug(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Debug, sourceName, correlationId, message, customProperties);
        }

        public static void LogInformation(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Information, sourceName, correlationId, message, customProperties);
        }

        public static void LogWarning(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Warning, sourceName, correlationId, message, customProperties);
        }

        public static void LogError(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Error, sourceName, correlationId, message, customProperties);
        }

        public static void LogCritical(this ILogger logger, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
        {
            logger.Log(LogLevel.Critical, sourceName, correlationId, message, customProperties);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, string sourceName, string correlationId, string message, IDictionary<string, object>? customProperties = null)
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

            using (logger.BeginScope(customProperties))
            {
                logger.Log(logLevel, message: FormatMessage(sourceName, correlationId, message));
            }
        }

        private static string FormatMessage(string sourceName, string correlationId, string message)
        {
            return $"{sourceName} ({correlationId}). {message.TrimEnd('.')}.";
        }
    }
}