using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace WoWMarketWatcher.API.Core
{
    public class ProblemDetailsWithErrors : ProblemDetails
    {
        public IEnumerable<string> Errors { get; set; } = new List<string>();

        private readonly Dictionary<int, string> errorTypes = new()
        {
            { 400, "https://tools.ietf.org/html/rfc7231#section-6.5.1" },
            { 401, "https://tools.ietf.org/html/rfc7235#section-3.1" },
            { 403, "https://tools.ietf.org/html/rfc7231#section-6.5.3" },
            { 404, "https://tools.ietf.org/html/rfc7231#section-6.5.4" },
            { 405, "https://tools.ietf.org/html/rfc7231#section-6.5.5" },
            { 500, "https://tools.ietf.org/html/rfc7231#section-6.6.1" }
        };

        private readonly Dictionary<int, string> errorTitles = new()
        {
            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 405, "Method Not Allowed" },
            { 500, "Internal Server Error" }
        };


        public ProblemDetailsWithErrors(IList<string> errors, int statusCode, HttpRequest? request = null)
        {
            this.SetProblemDetails(errors, statusCode, request);
        }

        public ProblemDetailsWithErrors(string error, int statusCode, HttpRequest? request = null) :
            this(new List<string> { error }, statusCode, request)
        { }

        public ProblemDetailsWithErrors(IList<string> errors, HttpStatusCode statusCode, HttpRequest? request = null)
        {
            this.SetProblemDetails(errors, (int)statusCode, request);
        }

        public ProblemDetailsWithErrors(string error, HttpStatusCode statusCode, HttpRequest? request = null) :
            this(new List<string> { error }, statusCode, request)
        { }

        public ProblemDetailsWithErrors(Exception error, int statusCode, HttpRequest? request = null)
        {
            var errors = new List<string> { error.Message };

            if (error is AggregateException)
            {
                var aggEx = error as AggregateException ?? throw new ArgumentException(null, nameof(error));
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

        public ProblemDetailsWithErrors(IList<string> errors, HttpRequest? request = null) : this(errors, 500, request) { }

        public ProblemDetailsWithErrors(string error, HttpRequest? request = null) : this(new List<string> { error }, 500, request) { }

        public ProblemDetailsWithErrors(Exception error, HttpRequest? request = null) : this(error, 500, request) { }

        private void SetProblemDetails(IList<string> errors, int statusCode, HttpRequest? request)
        {
            this.Errors = errors;
            this.Detail = errors.Count > 0 ? errors[0] : "Unknown error.";
            this.Status = statusCode;
            this.Title = this.errorTitles.ContainsKey(statusCode) ? this.errorTitles[statusCode] : "There was an error.";
            this.Instance = request != null ? $"{request.Method}: {request.GetDisplayUrl()}" : "";
            this.Type = this.errorTypes.ContainsKey(statusCode) ? this.errorTypes[statusCode] : "https://tools.ietf.org/html/rfc7231";
        }
    }
}