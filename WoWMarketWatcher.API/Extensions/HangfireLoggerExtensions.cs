using Hangfire.JobsLogger;
using Microsoft.Extensions.Logging;

namespace WoWMarketWatcher.API.Extensions
{
    public static class HangfireLoggerExtensions
    {
        public static void LogTrace(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        public static void LogDebug(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        public static void LogInformation(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Information, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        public static void LogWarning(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Warning, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        public static void LogError(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Error, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        public static void LogCritical(this ILogger logger, string hangfireJobId, string sourceName, string correlationId, string message, params object[] args)
        {
            logger.Log(LogLevel.Critical, jobId: hangfireJobId, FormatMessage(sourceName, correlationId, message), hangfireJobId, sourceName, correlationId, args);
        }

        private static string FormatMessage(string sourceName, string correlationId, string message)
        {
            return $"{sourceName} ({correlationId}). {message.TrimEnd('.')}.";
        }
    }
}