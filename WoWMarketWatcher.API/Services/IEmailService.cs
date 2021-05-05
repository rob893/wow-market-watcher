using System.Threading.Tasks;

namespace WoWMarketWatcher.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string correlationId, string toEmail, string subject, string message);
    }
}