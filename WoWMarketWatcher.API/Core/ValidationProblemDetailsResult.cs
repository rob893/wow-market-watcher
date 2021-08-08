using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WoWMarketWatcher.API.Core
{
    public sealed class ValidationProblemDetailsResult : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(entry => entry.Value.Errors.Select(e => $"{entry.Key}: {e.ErrorMessage}"))
                .ToList();

            var problemDetails = new ProblemDetailsWithErrors(errors, StatusCodes.Status400BadRequest, context.HttpContext.Request)
            {
                Title = "One or more validation errors occurred."
            };

            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails, jsonSettings));
        }
    }
}