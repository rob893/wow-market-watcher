using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models;

namespace WoWMarketWatcher.API.Controllers
{
    public abstract class ServiceControllerBase : ControllerBase
    {
        [NonAction]
        public bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true)
        {
            return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == resource.UserId);
        }

        [NonAction]
        public bool IsUserAuthorizedForResource(int userIdInQuestion, bool isAdminAuthorized = true)
        {
            return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == userIdInQuestion);
        }

        [NonAction]
        public NotFoundObjectResult NotFound(string errorMessage)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessage, 404, this.Request));
        }

        [NonAction]
        public NotFoundObjectResult NotFound(IList<string> errorMessages)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessages, 404, this.Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string errorMessage)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessage, 401, this.Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(IList<string> errorMessages)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, 401, this.Request));
        }

        [NonAction]
        public ObjectResult Forbidden(string errorMessage)
        {
            return base.StatusCode(403, new ProblemDetailsWithErrors(errorMessage, HttpStatusCode.Forbidden, this.Request));
        }

        [NonAction]
        public ObjectResult Forbidden(IList<string> errorMessages)
        {
            return base.StatusCode(403, new ProblemDetailsWithErrors(errorMessages, HttpStatusCode.Forbidden, this.Request));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(string errorMessage)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessage, 400, this.Request));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(IList<string> errorMessages)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, 400, this.Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(string errorMessage)
        {
            return base.StatusCode(500, new ProblemDetailsWithErrors(errorMessage, 500, this.Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(IList<string> errorMessages)
        {
            return base.StatusCode(500, new ProblemDetailsWithErrors(errorMessages, 500, this.Request));
        }

        internal string GetOrGenerateCorrelationId()
        {
            if (this.HttpContext.Request.Headers.TryGetCorrelationId(out var correlationId))
            {
                return correlationId;
            }

            return Guid.NewGuid().ToString();
        }
    }
}