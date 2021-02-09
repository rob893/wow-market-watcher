using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using System.Net;
using WoWMarketWatcher.Common.Models;

namespace WoWMarketWatcher.API.Controllers
{
    public abstract class ServiceControllerBase : ControllerBase
    {
        [NonAction]
        public bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true)
        {
            if (isAdminAuthorized && User.IsAdmin())
            {
                return true;
            }

            if (User.TryGetUserId(out int? userId) && userId == resource.UserId)
            {
                return true;
            }

            return false;
        }

        [NonAction]
        public bool IsUserAuthorizedForResource(int userIdInQuestion, bool isAdminAuthorized = true)
        {
            if (isAdminAuthorized && User.IsAdmin())
            {
                return true;
            }

            if (User.TryGetUserId(out int? userId) && userId == userIdInQuestion)
            {
                return true;
            }

            return false;
        }

        [NonAction]
        public NotFoundObjectResult NotFound(string errorMessage)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessage, 404, Request));
        }

        [NonAction]
        public NotFoundObjectResult NotFound(IList<string> errorMessages)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessages, 404, Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string errorMessage)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessage, 401, Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(IList<string> errorMessages)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, 401, Request));
        }

        [NonAction]
        public ObjectResult Forbidden(string errorMessage)
        {
            return base.StatusCode(403, new ProblemDetailsWithErrors(errorMessage, HttpStatusCode.Forbidden, Request));
        }

        [NonAction]
        public ObjectResult Forbidden(IList<string> errorMessages)
        {
            return base.StatusCode(403, new ProblemDetailsWithErrors(errorMessages, HttpStatusCode.Forbidden, Request));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(string errorMessage)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessage, 400, Request));
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(IList<string> errorMessages)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, 400, Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(string errorMessage)
        {
            return base.StatusCode(500, new ProblemDetailsWithErrors(errorMessage, 500, Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(IList<string> errorMessages)
        {
            return base.StatusCode(500, new ProblemDetailsWithErrors(errorMessages, 500, Request));
        }
    }
}