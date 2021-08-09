using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace WoWMarketWatcher.API.Utilities
{
    public static class UtilityFunctions
    {
        public static string GetSourceName(
            [CallerFilePath]
            string sourceFilePath = "",
            [CallerMemberName]
            string memberName = "")
        {
            var sourceName = string.Empty;
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                sourceName = sourceFilePath.Contains('\\')
                    ? sourceFilePath.Split('\\').Last().Split('.').First()
                    : sourceFilePath.Split('/').Last().Split('.').First();
            }

            return $"{sourceName}.{memberName}";
        }

        public static string GetControllerName<T>()
        {
            var typeName = typeof(T).Name;
            var splitOn = "Controller";

            if (typeName == null || !typeName.EndsWith(splitOn, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Controllers must end with 'Controller'. {typeName} does not.", nameof(T));
            }

            return typeName.Split(splitOn).First();
        }

        public static LogLevel LogLevelFromString(string logLevel)
        {
            if (logLevel == null)
            {
                throw new ArgumentNullException(nameof(logLevel));
            }

            return logLevel.ToUpperInvariant() switch
            {
                "TRACE" => LogLevel.Trace,
                "DEBUG" => LogLevel.Debug,
                "INFORMATION" => LogLevel.Information,
                "WARNING" => LogLevel.Warning,
                "ERROR" => LogLevel.Error,
                "CRITICAL" => LogLevel.Critical,
                _ => throw new ArgumentException($"{nameof(logLevel)} must be Trace, Debug, Information, Warning, Error, or Critical.", nameof(logLevel)),
            };
        }
    }
}