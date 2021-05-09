using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.Settings;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ISendGridClient client;
        private readonly SendGridSettings settings;
        private readonly ILogger<SendGridEmailService> logger;

        public SendGridEmailService(ISendGridClient client, IOptions<SendGridSettings> settings, ILogger<SendGridEmailService> logger)
        {
            this.client = client;
            this.settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            this.logger = logger;
        }

        public async Task SendEmailAsync(string correlationId, string toEmail, string subject, string message)
        {
            var sourceName = GetSourceName();

            if (!this.settings.Enabled)
            {
                this.logger.LogInformation(sourceName, correlationId, "Email sending is disabled. Returning.");
                return;
            }

            var msg = new SendGridMessage
            {
                From = new EmailAddress(this.settings.Sender, this.settings.SenderName),
                Subject = subject,
                PlainTextContent = message,
            };

            msg.AddTo(new EmailAddress(toEmail));

            var res = await this.client.SendEmailAsync(msg);

            if (!res.IsSuccessStatusCode)
            {
                var content = await res.Body.ReadAsStringAsync();
                this.logger.LogError(sourceName, correlationId, $"Unable to send email. Status: {res.StatusCode}. Reason: {content}");
                throw new HttpRequestException(content);
            }

            this.logger.LogInformation(sourceName, correlationId, "Sending email successful.");
        }
    }
}