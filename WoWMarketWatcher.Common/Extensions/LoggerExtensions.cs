using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WoWMarketWatcher.Common.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogTrace(this ILogger logger, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, FormatMessage(sourceName, correlationId, message), sourceName, correlationId, args);
        }

        public static void LogDebug(this ILogger logger, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, FormatMessage(sourceName, correlationId, message), sourceName, correlationId, args);
        }

        public static void LogInformation(this ILogger logger, string sourceName, string correlationId, string message, object? customProps = null)
        {
            if (customProps != null)
            {
                var customProperties = customProps.ToJson();
                logger.Log(LogLevel.Information, $"{FormatMessage(sourceName, correlationId, message)} {{customProperties}}", sourceName, correlationId, customProperties);
            }
            else
            {
                logger.Log(LogLevel.Information, $"{{sourceName}} ({{correlationId}}). {message.TrimEnd('.')}.", sourceName, correlationId);
            }
        }

        public static void LogWarning(this ILogger logger, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Warning, FormatMessage(sourceName, correlationId, message), sourceName, correlationId, args);
        }

        public static void LogError(this ILogger logger, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Error, FormatMessage(sourceName, correlationId, message), sourceName, correlationId, args);
        }

        public static void LogCritical(this ILogger logger, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Critical, FormatMessage(sourceName, correlationId, message), sourceName, correlationId, args);
        }

        private static string FormatMessage(string sourceName, string correlationId, string message)
        {
            return $"{{sourceName}} ({{correlationId}}). {message.TrimEnd('.').Replace("{", string.Empty).Replace("}", string.Empty)}.";
        }
    }
}