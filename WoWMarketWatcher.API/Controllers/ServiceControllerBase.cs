using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == resource.UserId);
        }

        [NonAction]
        public bool IsUserAuthorizedForResource(int userIdInQuestion, bool isAdminAuthorized = true)
        {
            return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == userIdInQuestion);
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(string errorMessage)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessage, StatusCodes.Status400BadRequest, this.Request));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(IList<string> errorMessages)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status400BadRequest, this.Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string errorMessage)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessage, StatusCodes.Status401Unauthorized, this.Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(IList<string> errorMessages)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status401Unauthorized, this.Request));
        }

        [NonAction]
        public ObjectResult Forbidden(string errorMessage)
        {
            return base.StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsWithErrors(errorMessage, StatusCodes.Status403Forbidden, this.Request));
        }

        [NonAction]
        public ObjectResult Forbidden(IList<string> errorMessages)
        {
            return base.StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status403Forbidden, this.Request));
        }

        [NonAction]
        public NotFoundObjectResult NotFound(string errorMessage)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessage, StatusCodes.Status404NotFound, this.Request));
        }

        [NonAction]
        public NotFoundObjectResult NotFound(IList<string> errorMessages)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status404NotFound, this.Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(string errorMessage)
        {
            return base.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsWithErrors(errorMessage, StatusCodes.Status500InternalServerError, this.Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(IList<string> errorMessages)
        {
            return base.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status500InternalServerError, this.Request));
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