using Core.Interfaces;
using Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Notifications
{
    public class SlackNotificationService : INotificationService
    {
        private readonly SlackTaskClient _client;
        private readonly string _channel;
        private readonly ILogger<SlackNotificationService> _logger;

        public SlackNotificationService(IConfiguration cfg, ILogger<SlackNotificationService> log)
        {
            _client  = new SlackTaskClient(cfg["Notifications:Slack:Token"]);
            _channel = cfg["Notifications:Slack:Channel"];
            _logger  = log;
        }

        public async Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            await _client.PostMessageAsync(
                _channel,
                $"*{message.Subject}*\n{message.Body}");
            _logger.LogInformation("Slack posted: {Subject}", message.Subject);
        }
    }
}
