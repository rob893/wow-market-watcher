using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly.Timeout;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;

using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;


        public GlobalExceptionHandlerMiddleware(RequestDelegate _, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var error = context.Features.Get<IExceptionHandlerFeature>();

            if (error != null)
            {
                var sourceName = GetSourceName();
                var thrownException = error.Error;
                var correlationId = context.Request.Headers.GetOrGenerateCorrelationId();
                var statusCode = StatusCodes.Status500InternalServerError;

                switch (thrownException)
                {
                    case TimeoutRejectedException:
                    case TimeoutException:
                        statusCode = StatusCodes.Status504GatewayTimeout;
                        break;
                    default:
                        break;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var problemDetails = new ProblemDetailsWithErrors(thrownException, context.Response.StatusCode, context.Request);

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                if (statusCode >= StatusCodes.Status500InternalServerError)
                {
                    this.logger.LogError(sourceName, correlationId, thrownException.Message);
                }
                else
                {
                    this.logger.LogWarning(sourceName, correlationId, thrownException.Message);
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
            }
        }
    }
}