using Core.Interfaces;
using Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Notifications
{
    public class CompositeNotificationService : INotificationService
    {
        private readonly IEnumerable<INotificationService> _providers;

        public CompositeNotificationService(IEnumerable<INotificationService> providers)
        {
            _providers = providers;
        }

        public async Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            foreach (var p in _providers)
                await p.NotifyAsync(message, cancellationToken);
        }
    }
}
