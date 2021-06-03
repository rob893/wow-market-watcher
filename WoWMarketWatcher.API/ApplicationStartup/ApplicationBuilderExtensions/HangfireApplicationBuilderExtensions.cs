using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Heartbeat.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using WoWMarketWatcher.API.BackgroundJobs;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions
{
    public static class HangfireApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAndConfigureHangfire(this IApplicationBuilder app, IRecurringJobManager recurringJobs, IConfiguration config)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (recurringJobs == null)
            {
                throw new ArgumentNullException(nameof(recurringJobs));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var backgroundJobSettings = config.GetSection(ConfigurationKeys.BackgroundJobs).Get<BackgroundJobSettings>();

            app.UseHangfireServer(additionalProcesses: new[] { new ProcessMonitor(TimeSpan.FromSeconds(1.5)) });
            app.UseHangfireDashboard(
                "/hangfire",
                new DashboardOptions
                {
                    DashboardTitle = $"{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName} - {config.GetEnvironment()} ({Assembly.GetExecutingAssembly().GetName().Version})",
                    Authorization = new[] { new DashboardAuthorizationFilter(config) }
                }
            );

            if (backgroundJobSettings.PullAuctionDataBackgroundJob.Enabled)
            {
                recurringJobs.AddOrUpdate<PullAuctionDataBackgroundJob>(nameof(PullAuctionDataBackgroundJob), job => job.PullAuctionData(null!), backgroundJobSettings.PullAuctionDataBackgroundJob.Schedule);
            }
            else
            {
                recurringJobs.RemoveIfExists(nameof(PullAuctionDataBackgroundJob));
            }

            if (backgroundJobSettings.RemoveOldDataBackgroundJob.Enabled)
            {
                recurringJobs.AddOrUpdate<RemoveOldDataBackgroundJob>(nameof(RemoveOldDataBackgroundJob), job => job.RemoveOldData(null!), backgroundJobSettings.RemoveOldDataBackgroundJob.Schedule);
            }
            else
            {
                recurringJobs.RemoveIfExists(nameof(RemoveOldDataBackgroundJob));
            }

            if (backgroundJobSettings.PullRealmDataBackgroundJob.Enabled)
            {
                recurringJobs.AddOrUpdate<PullRealmDataBackgroundJob>(nameof(PullRealmDataBackgroundJob), job => job.PullRealmData(null!), backgroundJobSettings.PullRealmDataBackgroundJob.Schedule);
            }
            else
            {
                recurringJobs.RemoveIfExists(nameof(PullRealmDataBackgroundJob));
            }

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
                if (!this.configuration.GetValue<bool>("Hangfire:Dashboard:Enabled"))
                {
                    return false;
                }

                if (!this.configuration.GetValue<bool>("Hangfire:Dashboard:RequireAuth"))
                {
                    return true;
                }

                var httpContext = context.GetHttpContext();

                if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) && authHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    // Get the encoded username and password
                    var encodedUsernamePassword = authHeader.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                    // Decode from Base64 to string
                    var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword ?? ""));

                    // Split username and password
                    var username = decodedUsernamePassword.Split(':', 2)[0];
                    var password = decodedUsernamePassword.Split(':', 2)[1];

                    // Check if login is correct
                    if (username == this.configuration[ConfigurationKeys.HangfireDashboardUsername] && password == this.configuration[ConfigurationKeys.HangfireDashboardPassword])
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