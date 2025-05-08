using Core.Interfaces;
using Core.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Notifications
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IConfiguration cfg, ILogger<EmailNotificationService> log)
        {
            _config = cfg;
            _logger   = log;
        }

        public async Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(_config["Notifications:Email:From"]));
            mail.To.Add(MailboxAddress.Parse(_config["Notifications:Email:To"]));
            mail.Subject = message.Subject;
            mail.Body    = new TextPart("plain") { Text = message.Body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Notifications:Email:SmtpServer"],
                int.Parse(_config["Notifications:Email:Port"]),
                true, cancellationToken);
            await client.AuthenticateAsync(
                _config["Notifications:Email:Username"],
                _config["Notifications:Email:Password"],
                cancellationToken);
            await client.SendAsync(mail, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent: {Subject}", message.Subject);
        }
    }
}
