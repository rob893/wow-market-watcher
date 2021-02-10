using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using WoWMarketWatcher.Common.Constants;
using WoWMarketWatcher.Common.Extensions;

namespace WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions
{
    public static class HangfireApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAndConfigureHangfire(this IApplicationBuilder app, IConfiguration config)
        {
            app.UseHangfireDashboard(
                "/hangfire",
                new DashboardOptions
                {
                    DashboardTitle = $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} - {config.GetEnvironment()} ({Assembly.GetExecutingAssembly().GetName().Version})",
                    Authorization = new[] { new DashboardAuthorizationFilter(config) }
                }
            );

            BackgroundJob.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));

            return app;
        }

        private class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
        {
            private readonly IConfiguration configuration;

            public DashboardAuthorizationFilter(IConfiguration configuration)
            {
                this.configuration = configuration;
            }

            public bool Authorize(DashboardContext context)
            {
                if (!configuration.GetEnvironment().Equals(ServiceEnvironment.Production, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var httpContext = context.GetHttpContext();

                string authHeader = httpContext.Request.Headers["Authorization"];
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
                    if (username == configuration["Hangfire:DashboardUsername"] && password == configuration["Hangfire:DashboardPassword"])
                    {
                        return true;
                    }
                }

                // Return authentication type (causes browser to show login dialog)
                httpContext.Response.Headers["WWW-Authenticate"] = "Basic";

                // Return unauthorized
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return false;
            }
        }
    }
}