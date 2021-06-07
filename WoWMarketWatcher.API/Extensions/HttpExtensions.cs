using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using WoWMarketWatcher.API.Constants;

namespace WoWMarketWatcher.API.Extensions
{
    public static class HttpExtensions
    {
        public static bool TryGetCorrelationId(this IHeaderDictionary headers, [NotNullWhen(true)] out string? correlationId)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

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
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

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
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (headers.TryGetValues(AppHeaderNames.CorrelationId, out var values) && values.Any())
            {
                return values.First();
            }

            return Guid.NewGuid().ToString();
        }

        public static string GetOrGenerateCorrelationId(this IHeaderDictionary headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (headers.TryGetValue(AppHeaderNames.CorrelationId, out var value))
            {
                return value.First();
            }

            return Guid.NewGuid().ToString();
        }
    }
}