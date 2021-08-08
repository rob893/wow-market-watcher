
using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using WoWMarketWatcher.API.Constants;

namespace WoWMarketWatcher.API.Core
{
    public sealed class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IConfiguration configuration;

        public ApplicationInsightsTelemetryInitializer(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            telemetry.Context.Cloud.RoleName = this.configuration[ConfigurationKeys.ApplicationInsightsCloudRoleName];
        }
    }
}