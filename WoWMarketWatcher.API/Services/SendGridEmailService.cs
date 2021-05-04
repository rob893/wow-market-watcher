using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using WoWMarketWatcher.API.Models.Settings;

namespace WoWMarketWatcher.API.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly SendGridSettings settings;

        public SendGridEmailService(IOptions<SendGridSettings> settings)
        {
            this.settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(this.settings.APIKey);

            var from = new EmailAddress("test@example.com", "Example User");
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, new EmailAddress(toEmail), subject, message, htmlContent);

            await client.SendEmailAsync(msg);
        }
    }
}