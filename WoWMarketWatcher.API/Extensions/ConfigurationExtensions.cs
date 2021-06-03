using System;
using Microsoft.Extensions.Configuration;

namespace WoWMarketWatcher.API.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetEnvironment(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var value = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT")?.Trim();
            return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"'ASPNETCORE_ENVIRONMENT' is not defined in configuration.") : value;
        }
    }
}