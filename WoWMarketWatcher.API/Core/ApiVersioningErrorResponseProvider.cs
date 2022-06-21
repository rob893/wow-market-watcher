using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace WoWMarketWatcher.API.Core
{
    public class ApiVersioningErrorResponseProvider : DefaultErrorResponseProvider
    {
        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var problemDetails = new ProblemDetailsWithErrors(context.Message ?? context.MessageDetail ?? "Unsupported API version.", StatusCodes.Status400BadRequest, context.Request);

            return new BadRequestObjectResult(problemDetails);
        }
    }
}