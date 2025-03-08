using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WoWMarketWatcher.API.Core
{
    public sealed class VersionHealthCheck : IHealthCheck
    {
        private readonly Version assemblyVersion;

        public VersionHealthCheck()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            this.assemblyVersion = entryAssembly?.GetName().Version ?? new Version(0, 0, 0, 0);
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var details = new Dictionary<string, object>
            {
                { "assemblyVersion", this.assemblyVersion }
            };

            return Task.FromResult(HealthCheckResult.Healthy($"The current deployed assembly version is {this.assemblyVersion}", details));
        }
    }
}