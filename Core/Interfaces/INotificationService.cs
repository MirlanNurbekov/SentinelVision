using Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// Send a single notification message via configured channels.
        /// </summary>
        Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
