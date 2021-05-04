using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.ApplicationStartup.ServiceCollectionExtensions
{
    public static class EmailServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendGridSettings>(config.GetSection(ConfigurationKeys.SendGrid));

            services.AddScoped<IEmailService, SendGridEmailService>();

            return services;
        }
    }
}