using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.Common.Extensions;
using WoWMarketWatcher.Common.Constants;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.Middleware
{
    public class SwaggerBasicAuthMiddleware
    {

        private readonly RequestDelegate next;

        public SwaggerBasicAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<SwaggerSettings> swaggerSettings, IConfiguration config)
        {
            var settings = swaggerSettings.Value;
            var authSettings = settings.AuthSettings;

            // Make sure we are hitting the swagger path
            if (context.Request.Path.StartsWithSegments("/swagger") && config.GetEnvironment().Equals(ServiceEnvironment.Production, StringComparison.OrdinalIgnoreCase))
            {
                if (!settings.Enabled)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }

                if (!authSettings.RequireAuth)
                {
                    await next.Invoke(context);
                    return;
                }

                string authHeader = context.Request.Headers["Authorization"];
                if (authHeader != null && authHeader.StartsWith("Basic "))
                {
                    // Get the encoded username and password
                    var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                    // Decode from Base64 to string
                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword ?? ""));

                    // Split username and password
                    var username = decodedUsernamePassword.Split(':', 2)[0];
                    var password = decodedUsernamePassword.Split(':', 2)[1];

                    // Check if login is correct
                    if (IsAuthorized(username, password, authSettings))
                    {
                        await next.Invoke(context);
                        return;
                    }
                }

                // Return authentication type (causes browser to show login dialog)
                context.Response.Headers["WWW-Authenticate"] = "Basic";

                // Return unauthorized
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context);
            }
        }

        public static bool IsAuthorized(string username, string password, SwaggerAuthSettings authSettings)
        {
            // Check that username and password are correct
            return username.Equals(authSettings.Username, StringComparison.InvariantCultureIgnoreCase) && password.Equals(authSettings.Password);
        }
    }
}