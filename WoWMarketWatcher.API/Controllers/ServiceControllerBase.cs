using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Core;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers
{
    public abstract class ServiceControllerBase : ControllerBase
    {
        private readonly ICorrelationIdService correlationIdService;

        protected ServiceControllerBase(ICorrelationIdService correlationIdService)
        {
            this.correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
        }

        protected string CorrelationId => this.correlationIdService.CorrelationId;

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
        public BadRequestObjectResult BadRequest(string errorMessage = "Bad request.")
        {
            return this.BadRequest(new List<string> { errorMessage });
        }

        [NonAction]
        public BadRequestObjectResult BadRequest(IEnumerable<string> errorMessages)
        {
            return base.BadRequest(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status400BadRequest, this.Request));
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(string errorMessage = "Unauthorized.")
        {
            return this.Unauthorized(new List<string> { errorMessage });
        }

        [NonAction]
        public UnauthorizedObjectResult Unauthorized(IEnumerable<string> errorMessages)
        {
            return base.Unauthorized(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status401Unauthorized, this.Request));
        }

        [NonAction]
        public ObjectResult Forbidden(string errorMessage = "Forbidden.")
        {
            return this.Forbidden(new List<string> { errorMessage });
        }

        [NonAction]
        public ObjectResult Forbidden(IEnumerable<string> errorMessages)
        {
            return base.StatusCode(StatusCodes.Status403Forbidden, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status403Forbidden, this.Request));
        }

        [NonAction]
        public NotFoundObjectResult NotFound(string errorMessage = "Resource not found.")
        {
            return this.NotFound(new List<string> { errorMessage });
        }

        [NonAction]
        public NotFoundObjectResult NotFound(IEnumerable<string> errorMessages)
        {
            return base.NotFound(new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status404NotFound, this.Request));
        }

        [NonAction]
        public ObjectResult InternalServerError(string errorMessage = "Internal server error.")
        {
            return this.InternalServerError(new List<string> { errorMessage });
        }

        [NonAction]
        public ObjectResult InternalServerError(IEnumerable<string> errorMessages)
        {
            return base.StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsWithErrors(errorMessages, StatusCodes.Status500InternalServerError, this.Request));
        }
    }
}