using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Extensions;

namespace WoWMarketWatcher.API.Core
{
    public class ProblemDetailsWithErrors : ProblemDetails
    {
        private readonly Dictionary<int, string> errorTypes = new()
        {
            { StatusCodes.Status400BadRequest, "https://tools.ietf.org/html/rfc7231#section-6.5.1" },
            { StatusCodes.Status401Unauthorized, "https://tools.ietf.org/html/rfc7235#section-3.1" },
            { StatusCodes.Status403Forbidden, "https://tools.ietf.org/html/rfc7231#section-6.5.3" },
            { StatusCodes.Status404NotFound, "https://tools.ietf.org/html/rfc7231#section-6.5.4" },
            { StatusCodes.Status405MethodNotAllowed, "https://tools.ietf.org/html/rfc7231#section-6.5.5" },
            { StatusCodes.Status500InternalServerError, "https://tools.ietf.org/html/rfc7231#section-6.6.1" }
        };

        private readonly Dictionary<int, string> errorTitles = new()
        {
            { StatusCodes.Status400BadRequest, "Bad Request" },
            { StatusCodes.Status401Unauthorized, "Unauthorized" },
            { StatusCodes.Status403Forbidden, "Forbidden" },
            { StatusCodes.Status404NotFound, "Not Found" },
            { StatusCodes.Status405MethodNotAllowed, "Method Not Allowed" },
            { StatusCodes.Status500InternalServerError, "Internal Server Error" }
        };

        public ProblemDetailsWithErrors(IList<string> errors, int statusCode, HttpRequest? request = null)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            this.SetProblemDetails(errors, statusCode, request);
        }

        public ProblemDetailsWithErrors(string error, int statusCode, HttpRequest? request = null) :
            this(new List<string> { error }, statusCode, request)
        { }

        public ProblemDetailsWithErrors(IList<string> errors, HttpStatusCode statusCode, HttpRequest? request = null)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            this.SetProblemDetails(errors, (int)statusCode, request);
        }

        public ProblemDetailsWithErrors(string error, HttpStatusCode statusCode, HttpRequest? request = null) :
            this(new List<string> { error }, statusCode, request)
        { }

        public ProblemDetailsWithErrors(Exception error, int statusCode, HttpRequest? request = null)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var errors = new List<string> { error.Message };

            if (error is AggregateException aggEx)
            {
                foreach (var innerException in aggEx.InnerExceptions)
                {
                    errors.Add(innerException.Message);
                }
            }
            else
            {
                var innerError = error.InnerException;
                while (innerError != null)
                {
                    errors.Add(innerError.Message);
                    innerError = innerError.InnerException;
                }
            }

            this.SetProblemDetails(errors, statusCode, request);
        }

        public ProblemDetailsWithErrors(IList<string> errors, HttpRequest? request = null) : this(errors, StatusCodes.Status500InternalServerError, request) { }

        public ProblemDetailsWithErrors(string error, HttpRequest? request = null) : this(new List<string> { error }, StatusCodes.Status500InternalServerError, request) { }

        public ProblemDetailsWithErrors(Exception error, HttpRequest? request = null) : this(error, StatusCodes.Status500InternalServerError, request) { }

        private void SetProblemDetails(IList<string> errors, int statusCode, HttpRequest? request)
        {
            var correlationId = request?.Headers.GetOrGenerateCorrelationId() ?? Guid.NewGuid().ToString();

            this.Detail = errors.Count > 0 ? errors[0] : "Unknown error.";
            this.Status = statusCode;
            this.Title = this.errorTitles.ContainsKey(statusCode) ? this.errorTitles[statusCode] : "There was an error.";
            this.Instance = request != null ? $"{request.Method}: {request.GetDisplayUrl()}" : "";
            this.Type = this.errorTypes.ContainsKey(statusCode) ? this.errorTypes[statusCode] : "https://tools.ietf.org/html/rfc7231";
            this.Extensions["correlationId"] = correlationId;
            this.Extensions["errors"] = errors;
            this.Extensions["traceId"] = Activity.Current?.Id ?? request?.HttpContext?.TraceIdentifier ?? correlationId;
        }
    }
}