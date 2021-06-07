using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WoWMarketWatcher.API.Core;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class ControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddControllerServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddControllers(options =>
            {
                // This allows for global authorization. No need to have [Authorize] attribute on controllers with this.
                // This is what requires tokens for all endpoints. Add [AllowAnonymous] to any endpoint not requiring tokens (like login)
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = _ => new ValidationProblemDetailsResult();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });

            return services;
        }
    }
}