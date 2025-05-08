using System;
using System.Collections.Generic;

namespace Core.Models
{
    /// <summary>
    /// Indicates delivery priority for a notification.
    /// </summary>
    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    /// <summary>
    /// Supported channels for sending notifications.
    /// </summary>
    public enum NotificationChannel
    {
        Email,
        Slack,
        Sms,
        Push
    }

    /// <summary>
    /// Represents a message to be sent via one or more notification channels.
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// Unique identifier for tracing this message across systems.
        /// </summary>
        public Guid CorrelationId { get; } = Guid.NewGuid();

        /// <summary>
        /// Destination channels for this message.
        /// </summary>
        public IReadOnlyCollection<NotificationChannel> Channels { get; }

        /// <summary>
        /// Human-readable subject or title.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Main content of the notification.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Recipients mapped by channel (e.g. email address, Slack channel ID).
        /// </summary>
        public IReadOnlyDictionary<NotificationChannel, List<string>> Recipients { get; }

        /// <summary>
        /// Optional metadata for routing, templating, or auditing.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// When the notification was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Delivery priority for this notification.
        /// </summary>
        public NotificationPriority Priority { get; }

        /// <summary>
        /// Initializes a new NotificationMessage.
        /// </summary>
        /// <param name="channels">Channels to deliver to.</param>
        /// <param name="recipients">Recipients per channel.</param>
        /// <param name="subject">Notification subject.</param>
        /// <param name="body">Notification body.</param>
        /// <param name="priority">Delivery priority.</param>
        /// <param name="metadata">Optional metadata.</param>
        public NotificationMessage(
            IEnumerable<NotificationChannel> channels,
            IDictionary<NotificationChannel, List<string>> recipients,
            string subject,
            string body,
            NotificationPriority priority = NotificationPriority.Normal,
            IDictionary<string, string> metadata = null)
        {
            if (channels == null) throw new ArgumentNullException(nameof(channels));
            if (recipients == null) throw new ArgumentNullException(nameof(recipients));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject cannot be empty.", nameof(subject));
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Body cannot be empty.", nameof(body));

            Channels = new List<NotificationChannel>(channels).AsReadOnly();
            Recipients = new Dictionary<NotificationChannel, List<string>>(recipients);
            Subject = subject;
            Body = body;
            Priority = priority;
            Metadata = metadata != null
                ? new Dictionary<string, string>(metadata)
                : new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds or updates a metadata entry.
        /// </summary>
        public void AddMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty.", nameof(key));
            ((IDictionary<string, string>)Metadata)[key] = value;
        }
    }
}
