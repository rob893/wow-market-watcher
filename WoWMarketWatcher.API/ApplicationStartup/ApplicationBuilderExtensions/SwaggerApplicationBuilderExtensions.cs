using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Middleware;
using WoWMarketWatcher.Common.Extensions;

namespace WoWMarketWatcher.API.ApplicationStartup.ApplicationBuilderExtensions
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAndConfigureSwagger(this IApplicationBuilder app, IConfiguration config)
        {
            app.UseMiddleware<SwaggerBasicAuthMiddleware>()
                .UseSwagger()
                .UseSwaggerUI(
                    c =>
                    {
                        c.SwaggerEndpoint(config[ConfigurationKeys.SwaggerEndpoint], FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName);
                        c.DocumentTitle = $"WoW Market Watcher - {config.GetEnvironment()}";
                    });

            return app;
        }
    }
}