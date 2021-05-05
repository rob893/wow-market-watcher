using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;
using SendGrid.Helpers.Reliability;
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
            var settings = config.GetSection(ConfigurationKeys.SendGrid).Get<SendGridSettings>();

            services.AddSendGrid(options =>
            {
                options.ApiKey = settings.APIKey;
                options.ReliabilitySettings = new ReliabilitySettings(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));
            });
            services.AddScoped<IEmailService, SendGridEmailService>();

            return services;
        }
    }
}