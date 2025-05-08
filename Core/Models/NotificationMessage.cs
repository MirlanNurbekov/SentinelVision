using System.Collections.Generic;

namespace Core.Models
{
    public enum NotificationChannel
    {
        Email,
        Slack
    }

    public class NotificationMessage
    {
        public NotificationChannel Channel { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IDictionary<string, string> Metadata { get; set; }  // e.g. { "UserId":"42" }
    }
}
