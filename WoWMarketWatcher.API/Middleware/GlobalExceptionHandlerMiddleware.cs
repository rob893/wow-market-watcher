using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.API.Core;

namespace WoWMarketWatcher.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> logger;


        public GlobalExceptionHandlerMiddleware(RequestDelegate _, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            this.logger = logger;
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
                var thrownException = error.Error;

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var problemDetails = new ProblemDetailsWithErrors(thrownException, context.Response.StatusCode, context.Request);

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                this.logger.LogError(thrownException.Message);

                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
            }
        }
    }
}