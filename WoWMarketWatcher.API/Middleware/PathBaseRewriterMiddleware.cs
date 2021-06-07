using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WoWMarketWatcher.API.Constants;

namespace WoWMarketWatcher.API.Middleware
{
    /// <summary>
    /// This middleware rewrites the request path base to whatever is in the X-Forwarded-Prefix.
    /// Useful if the app is running behind a reverse proxy or load balancer.
    /// </summary>
    public class PathBaseRewriterMiddleware
    {
        private readonly RequestDelegate next;

        public PathBaseRewriterMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.Headers.TryGetValue(AppHeaderNames.ForwardedPrefix, out var value))
            {
                context.Request.PathBase = value.First();
            }

            await this.next(context);
        }
    }
}