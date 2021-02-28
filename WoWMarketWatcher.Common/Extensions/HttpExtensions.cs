using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using WoWMarketWatcher.Common.Constants;

namespace WoWMarketWatcher.Common.Extensions
{
    public static class HttpExtensions
    {
        public static bool TryGetCorrelationId(this IHeaderDictionary headers, [NotNullWhen(true)] out string? correlationId)
        {
            correlationId = null;

            if (headers.TryGetValue(AppHeaderNames.CorrelationId, out var value))
            {
                correlationId = value;
                return true;
            }

            return false;
        }

        public static bool TryGetCorrelationId(this HttpHeaders headers, [NotNullWhen(true)] out string? correlationId)
        {
            correlationId = null;

            if (headers.TryGetValues(AppHeaderNames.CorrelationId, out var values) && values.Any())
            {
                correlationId = values.First();
                return true;
            }

            return false;
        }

        public static string GetOrGenerateCorrelationId(this HttpHeaders headers)
        {
            if (headers.TryGetValues(AppHeaderNames.CorrelationId, out var values) && values.Any())
            {
                return values.First();
            }

            return Guid.NewGuid().ToString();
        }
    }
}