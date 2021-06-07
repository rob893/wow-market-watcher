using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Middleware
{
    /// <summary>
    /// This middleware adds the correlation id passed in the request into the response.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger<CorrelationIdMiddleware> logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sourceName = GetSourceName();

            if (!context.Request.Headers.TryGetCorrelationId(out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Add(AppHeaderNames.CorrelationId, correlationId);
                this.logger.LogDebug(sourceName, correlationId, "No correlation id found in request headers. Generating a new one. It will be added to the response headers.");
            }
            else
            {
                this.logger.LogDebug(sourceName, correlationId, "Correlation id found in request headers. It will be added to the response headers.");
            }

            context.Response.OnStarting(() =>
            {
                // Remove previous correlation id as passed correlation id always takes priority.
                if (context.Response.Headers.TryGetCorrelationId(out var currentCorrelationId))
                {
                    this.logger.LogDebug(sourceName, correlationId, $"Correlation id of {currentCorrelationId} has already been added to response headers. Removing in favor of correlation id from client.");
                    context.Response.Headers.Remove(AppHeaderNames.CorrelationId);
                }

                context.Response.Headers.Add(AppHeaderNames.CorrelationId, correlationId);

                this.logger.LogDebug(sourceName, correlationId, "Correlation id added to the response headers.");

                return Task.CompletedTask;
            });


            await this.next(context);
        }
    }
}