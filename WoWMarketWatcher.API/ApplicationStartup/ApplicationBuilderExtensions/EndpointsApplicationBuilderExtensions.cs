using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using WoWMarketWatcher.API.Constants;

namespace WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions
{
    public static class EndpointsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAndConfigureEndpoints(this IApplicationBuilder app, IConfiguration config)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks(ApplicationSettings.HealthCheckEndpoint, new HealthCheckOptions()
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    endpoints.MapHealthChecks(ApplicationSettings.LivenessHealthCheckEndpoint, new HealthCheckOptions()
                    {
                        Predicate = (check) => !check.Tags.Contains(HealthCheckTags.Dependency),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    endpoints.MapControllers();
                });

            return app;
        }
    }
}